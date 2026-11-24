namespace GjejVend.Infrastructure.Payments;

public sealed record GatewayPaymentRequest(
    Guid PaymentId, Guid BookingId, decimal Amount, string Currency, string Description, string? CustomerEmail = null);

public sealed record GatewayCheckout(string Reference, string CheckoutUrl);

public enum GatewayEventType { PaymentSucceeded, PaymentFailed, Ignored }

/// <summary>A verified provider event. It identifies the payment by session reference and/or our payment id.</summary>
public sealed record GatewayWebhookEvent(GatewayEventType Type, string? Reference, Guid? PaymentId, string? FailureReason);

public sealed record GatewayInvoice(string? Number, string? HostedUrl, string? PdfUrl);

/// <summary>Charges, refunds, invoices and webhooks. Implemented by Stripe; a signed fake is used in development.</summary>
public interface IPaymentGateway
{
    string Name { get; }

    Task<GatewayCheckout> CreatePaymentAsync(GatewayPaymentRequest request, CancellationToken ct = default);

    /// <param name="paymentReference">The reference returned by <see cref="CreatePaymentAsync"/>.</param>
    /// <param name="idempotencyKey">Makes retries safe: the same key never refunds twice.</param>
    Task<string> RefundAsync(string paymentReference, decimal amount, string reason, string idempotencyKey, CancellationToken ct = default);

    /// <summary>The provider-hosted invoice for a paid payment, or null when it does not exist (yet).</summary>
    Task<GatewayInvoice?> GetInvoiceAsync(string paymentReference, CancellationToken ct = default);

    /// <summary>Verifies the signature and parses the body. Returns null when the signature or payload is invalid.</summary>
    GatewayWebhookEvent? ParseWebhook(string body, string? signature);
}

/// <summary>Provider payouts: connected accounts for owners and transfers to them.</summary>
public interface IPayoutGateway
{
    Task<string> CreateConnectedAccountAsync(Guid providerId, string? email, CancellationToken ct = default);

    Task<string> CreateOnboardingLinkAsync(string accountId, string returnUrl, string refreshUrl, CancellationToken ct = default);

    /// <returns>The provider's transfer reference.</returns>
    Task<string> TransferAsync(string accountId, decimal amount, string currency, string description,
        string idempotencyKey, CancellationToken ct = default);
}

/// <summary>Settings for the payment provider, bound from configuration.</summary>
public sealed record PaymentsSettings(
    string? StripeSecretKey,
    string? StripeWebhookSecret,
    string FrontendBaseUrl,
    string FakeWebhookSecret)
{
    public bool UseStripe => !string.IsNullOrWhiteSpace(StripeSecretKey);
}
