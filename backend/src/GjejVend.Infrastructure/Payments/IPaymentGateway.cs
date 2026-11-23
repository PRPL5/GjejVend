namespace GjejVend.Infrastructure.Payments;

public sealed record GatewayPaymentRequest(Guid PaymentId, Guid BookingId, decimal Amount, string Currency, string Description);

public sealed record GatewayCheckout(string Reference, string CheckoutUrl);

public enum GatewayEventType { PaymentSucceeded, PaymentFailed }

public sealed record GatewayWebhookEvent(GatewayEventType Type, string Reference, string? FailureReason);

/// <summary>
/// The external payment provider. M3 only talks to this interface, so the provider can be swapped
/// once one that supports Kosovo merchants is chosen.
/// </summary>
public interface IPaymentGateway
{
    string Name { get; }

    Task<GatewayCheckout> CreatePaymentAsync(GatewayPaymentRequest request, CancellationToken ct = default);

    Task<string> RefundAsync(string paymentReference, decimal amount, string reason, CancellationToken ct = default);

    /// <summary>Verifies the signature and parses the body. Returns null when the signature or payload is invalid.</summary>
    GatewayWebhookEvent? ParseWebhook(string body, string? signature);
}
