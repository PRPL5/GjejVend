namespace GjejVend.Application.Payments;

public sealed record GatewayPaymentRequest(Guid PaymentId, Guid BookingId, decimal Amount, string Currency, string Description);

public sealed record GatewayCheckout(string Reference, string CheckoutUrl);

public enum GatewayEventType { PaymentSucceeded, PaymentFailed }

public sealed record GatewayWebhookEvent(GatewayEventType Type, string Reference, string? FailureReason);

/// <summary>Abstraction over the external payment provider (test mode until a Kosovo provider is chosen).</summary>
public interface IPaymentGateway
{
    string Name { get; }
    Task<GatewayCheckout> CreatePaymentAsync(GatewayPaymentRequest request, CancellationToken ct = default);
    Task<string> RefundAsync(string paymentReference, decimal amount, string reason, CancellationToken ct = default);
    Task<string> PayoutAsync(Guid providerId, decimal amount, string currency, CancellationToken ct = default);

    /// <summary>Verifies the webhook signature and parses the body. Returns null if the signature is invalid.</summary>
    GatewayWebhookEvent? ParseWebhook(string body, string? signature);
}
