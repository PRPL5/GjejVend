using Stripe;
using Stripe.Checkout;

namespace GjejVend.Infrastructure.Payments;

/// <summary>
/// Stripe implementation: Checkout Sessions (with invoice creation) for charges, Refunds, and Connect
/// transfers for provider payouts. All amounts are converted to the smallest currency unit (cents).
/// </summary>
public sealed class StripePaymentGateway : IPaymentGateway, IPayoutGateway
{
    public const string ProviderName = "stripe";

    private readonly string _webhookSecret;
    private readonly string _frontendBaseUrl;
    private readonly SessionService _sessions;
    private readonly RefundService _refunds;
    private readonly TransferService _transfers;
    private readonly AccountService _accounts;
    private readonly AccountLinkService _accountLinks;

    public StripePaymentGateway(PaymentsSettings settings)
    {
        var secretKey = settings.StripeSecretKey ?? throw new ArgumentException("Stripe secret key is required.", nameof(settings));
        _webhookSecret = settings.StripeWebhookSecret ?? throw new ArgumentException("Stripe webhook secret is required.", nameof(settings));
        _frontendBaseUrl = settings.FrontendBaseUrl.TrimEnd('/');

        var client = new StripeClient(secretKey);
        _sessions = new SessionService(client);
        _refunds = new RefundService(client);
        _transfers = new TransferService(client);
        _accounts = new AccountService(client);
        _accountLinks = new AccountLinkService(client);
    }

    public string Name => ProviderName;

    public async Task<GatewayCheckout> CreatePaymentAsync(GatewayPaymentRequest request, CancellationToken ct = default)
    {
        var metadata = new Dictionary<string, string>
        {
            ["payment_id"] = request.PaymentId.ToString(),
            ["booking_id"] = request.BookingId.ToString(),
        };

        var session = await _sessions.CreateAsync(new SessionCreateOptions
        {
            Mode = "payment",
            ClientReferenceId = request.BookingId.ToString(),
            CustomerEmail = request.CustomerEmail,
            SuccessUrl = $"{_frontendBaseUrl}/bookings/{request.BookingId}/pay?payment=success&session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{_frontendBaseUrl}/bookings/{request.BookingId}/pay?payment=cancelled",
            Metadata = metadata,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.Currency.ToLowerInvariant(),
                        UnitAmount = ToMinorUnits(request.Amount),
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = request.Description },
                    },
                },
            ],
            PaymentIntentData = new SessionPaymentIntentDataOptions { Metadata = metadata },
            // Stripe creates and hosts the invoice (and PDF) once the payment completes.
            InvoiceCreation = new SessionInvoiceCreationOptions
            {
                Enabled = true,
                InvoiceData = new SessionInvoiceCreationInvoiceDataOptions { Description = request.Description, Metadata = metadata },
            },
        }, new RequestOptions { IdempotencyKey = $"checkout-{request.PaymentId}" }, ct);

        return new GatewayCheckout(session.Id, session.Url);
    }

    public async Task<string> RefundAsync(string paymentReference, decimal amount, string reason,
        string idempotencyKey, CancellationToken ct = default)
    {
        var session = await _sessions.GetAsync(paymentReference, cancellationToken: ct);
        var refund = await _refunds.CreateAsync(new RefundCreateOptions
        {
            PaymentIntent = session.PaymentIntentId
                            ?? throw new InvalidOperationException("The checkout session has no payment intent to refund."),
            Amount = ToMinorUnits(amount),
            Reason = "requested_by_customer",
            Metadata = new Dictionary<string, string> { ["reason"] = reason },
        }, new RequestOptions { IdempotencyKey = $"refund-{idempotencyKey}" }, ct);
        return refund.Id;
    }

    public async Task<GatewayInvoice?> GetInvoiceAsync(string paymentReference, CancellationToken ct = default)
    {
        var session = await _sessions.GetAsync(paymentReference, new SessionGetOptions { Expand = ["invoice"] }, cancellationToken: ct);
        var invoice = session.Invoice;
        return invoice is null ? null : new GatewayInvoice(invoice.Number, invoice.HostedInvoiceUrl, invoice.InvoicePdf);
    }

    public GatewayWebhookEvent? ParseWebhook(string body, string? signature)
    {
        if (string.IsNullOrEmpty(signature)) return null;

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(body, signature, _webhookSecret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException)
        {
            return null; // bad signature, stale timestamp or malformed payload
        }

        switch (stripeEvent.Data.Object)
        {
            case Session session when stripeEvent.Type is EventTypes.CheckoutSessionCompleted
                                          or EventTypes.CheckoutSessionAsyncPaymentSucceeded:
                // Completed with payment_status "unpaid" means a delayed method is still pending.
                return session.PaymentStatus == "paid"
                    ? new GatewayWebhookEvent(GatewayEventType.PaymentSucceeded, session.Id, ParseId(session.Metadata), null)
                    : Ignored();

            case Session session when stripeEvent.Type is EventTypes.CheckoutSessionAsyncPaymentFailed
                                          or EventTypes.CheckoutSessionExpired:
                return new GatewayWebhookEvent(GatewayEventType.PaymentFailed, session.Id, ParseId(session.Metadata),
                    stripeEvent.Type == EventTypes.CheckoutSessionExpired ? "checkout expired" : "payment failed");

            case PaymentIntent intent when stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed:
                return new GatewayWebhookEvent(GatewayEventType.PaymentFailed, null, ParseId(intent.Metadata),
                    intent.LastPaymentError?.Message ?? "payment failed");

            default:
                return Ignored();
        }
    }

    public async Task<string> CreateConnectedAccountAsync(Guid providerId, string? email, CancellationToken ct = default)
    {
        var account = await _accounts.CreateAsync(new AccountCreateOptions
        {
            Type = "express",
            Email = email,
            Metadata = new Dictionary<string, string> { ["provider_id"] = providerId.ToString() },
            Capabilities = new AccountCapabilitiesOptions
            {
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
            },
        }, new RequestOptions { IdempotencyKey = $"connect-account-{providerId}" }, ct);
        return account.Id;
    }

    public async Task<string> CreateOnboardingLinkAsync(string accountId, string returnUrl, string refreshUrl,
        CancellationToken ct = default)
    {
        var link = await _accountLinks.CreateAsync(new AccountLinkCreateOptions
        {
            Account = accountId,
            ReturnUrl = returnUrl,
            RefreshUrl = refreshUrl,
            Type = "account_onboarding",
        }, cancellationToken: ct);
        return link.Url;
    }

    public async Task<string> TransferAsync(string accountId, decimal amount, string currency, string description,
        string idempotencyKey, CancellationToken ct = default)
    {
        var transfer = await _transfers.CreateAsync(new TransferCreateOptions
        {
            Amount = ToMinorUnits(amount),
            Currency = currency.ToLowerInvariant(),
            Destination = accountId,
            Description = description,
        }, new RequestOptions { IdempotencyKey = $"transfer-{idempotencyKey}" }, ct);
        return transfer.Id;
    }

    private static GatewayWebhookEvent Ignored() => new(GatewayEventType.Ignored, null, null, null);

    private static Guid? ParseId(Dictionary<string, string>? metadata)
        => metadata is not null && metadata.TryGetValue("payment_id", out var value) && Guid.TryParse(value, out var id) ? id : null;

    private static long ToMinorUnits(decimal amount) => (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
}
