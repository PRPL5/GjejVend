using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GjejVend.Application.Payments;

namespace GjejVend.Infrastructure.Payments;

public sealed class PaymentOptions
{
    public const string Section = "Payments";
    public string WebhookSecret { get; set; } = "dev-webhook-secret-change-me";
}

/// <summary>
/// Test-mode gateway used until the real provider is chosen. It never moves money; it only
/// issues references and verifies HMAC-SHA256 signed webhooks, like a real provider would.
/// </summary>
public sealed class FakePaymentGateway(PaymentOptions options) : IPaymentGateway
{
    public string Name => "fake";

    public Task<GatewayCheckout> CreatePaymentAsync(GatewayPaymentRequest request, CancellationToken ct = default)
    {
        var reference = $"fake_pay_{request.PaymentId:N}";
        return Task.FromResult(new GatewayCheckout(reference, $"/fake-checkout/{reference}"));
    }

    public Task<string> RefundAsync(string paymentReference, decimal amount, string reason, CancellationToken ct = default)
        => Task.FromResult($"fake_ref_{Guid.NewGuid():N}");

    public Task<string> PayoutAsync(Guid providerId, decimal amount, string currency, CancellationToken ct = default)
        => Task.FromResult($"fake_po_{Guid.NewGuid():N}");

    public GatewayWebhookEvent? ParseWebhook(string body, string? signature)
    {
        if (string.IsNullOrEmpty(signature) || !VerifySignature(body, signature)) return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();
            var reference = root.GetProperty("reference").GetString();
            if (string.IsNullOrEmpty(reference)) return null;
            var reason = root.TryGetProperty("reason", out var r) ? r.GetString() : null;
            return type switch
            {
                "payment.succeeded" => new GatewayWebhookEvent(GatewayEventType.PaymentSucceeded, reference, null),
                "payment.failed" => new GatewayWebhookEvent(GatewayEventType.PaymentFailed, reference, reason),
                _ => null,
            };
        }
        catch (JsonException)
        {
            return null;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public string Sign(string body)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(options.WebhookSecret), Encoding.UTF8.GetBytes(body));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private bool VerifySignature(string body, string signature)
        => CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(Sign(body)), Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
}
