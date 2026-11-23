using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GjejVend.Infrastructure.Payments;

/// <summary>
/// Test-mode gateway. It never moves money: it issues references and verifies HMAC-SHA256 signed
/// webhooks the way a real provider would, so the whole payment flow can be exercised end to end.
/// </summary>
public sealed class FakePaymentGateway(string webhookSecret) : IPaymentGateway
{
    public const string ProviderName = "fake";

    public string Name => ProviderName;

    public Task<GatewayCheckout> CreatePaymentAsync(GatewayPaymentRequest request, CancellationToken ct = default)
    {
        var reference = $"fake_pay_{request.PaymentId:N}";
        return Task.FromResult(new GatewayCheckout(reference, $"/fake-checkout/{reference}"));
    }

    public Task<string> RefundAsync(string paymentReference, decimal amount, string reason, CancellationToken ct = default)
        => Task.FromResult($"fake_ref_{Guid.NewGuid():N}");

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
                "payment.succeeded" => new GatewayWebhookEvent(GatewayEventType.PaymentSucceeded, reference, null),
                "payment.failed" => new GatewayWebhookEvent(GatewayEventType.PaymentFailed, reference, reason),
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
