using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GjejVend.Infrastructure.Payments;

/// <summary>
/// Development and test gateway used when no Stripe key is configured. It never moves money: it issues
/// references and verifies HMAC-SHA256 signed webhooks, so the whole flow can run without Stripe.
/// </summary>
public sealed class FakePaymentGateway(string webhookSecret) : IPaymentGateway, IPayoutGateway
{
    public const string ProviderName = "fake";

    public string Name => ProviderName;

    public Task<GatewayCheckout> CreatePaymentAsync(GatewayPaymentRequest request, CancellationToken ct = default)
    {
        var reference = $"fake_pay_{request.PaymentId:N}";
        return Task.FromResult(new GatewayCheckout(reference, $"/fake-checkout/{reference}"));
    }

    public Task<string> RefundAsync(string paymentReference, decimal amount, string reason,
        string idempotencyKey, CancellationToken ct = default)
        => Task.FromResult($"fake_ref_{idempotencyKey}");

    public Task<GatewayInvoice?> GetInvoiceAsync(string paymentReference, CancellationToken ct = default)
        => Task.FromResult<GatewayInvoice?>(new GatewayInvoice(
            $"FAKE-{paymentReference[^6..].ToUpperInvariant()}", $"/fake-invoice/{paymentReference}", null));

    public Task<string> CreateConnectedAccountAsync(Guid providerId, string? email, CancellationToken ct = default)
        => Task.FromResult($"acct_fake_{providerId:N}");

    public Task<string> CreateOnboardingLinkAsync(string accountId, string returnUrl, string refreshUrl, CancellationToken ct = default)
        => Task.FromResult(returnUrl);

    public Task<string> TransferAsync(string accountId, decimal amount, string currency, string description,
        string idempotencyKey, CancellationToken ct = default)
        => Task.FromResult($"fake_tr_{idempotencyKey}");

    public GatewayWebhookEvent? ParseWebhook(string body, string? signature)
    {
        if (string.IsNullOrEmpty(signature) || !SignatureMatches(body, signature)) return null;

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var reference = root.GetProperty("reference").GetString();
            if (string.IsNullOrEmpty(reference)) return null;

            var reason = root.TryGetProperty("reason", out var r) ? r.GetString() : null;
            return root.GetProperty("type").GetString() switch
            {
                "payment.succeeded" => new GatewayWebhookEvent(GatewayEventType.PaymentSucceeded, reference, null, null),
                "payment.failed" => new GatewayWebhookEvent(GatewayEventType.PaymentFailed, reference, null, reason),
                _ => null,
            };
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>Signs a webhook body. Used by tests and by the local simulator.</summary>
    public string Sign(string body)
        => Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(webhookSecret), Encoding.UTF8.GetBytes(body)))
            .ToLowerInvariant();

    private bool SignatureMatches(string body, string signature)
        => CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(Sign(body)), Encoding.UTF8.GetBytes(signature.Trim().ToLowerInvariant()));
}
