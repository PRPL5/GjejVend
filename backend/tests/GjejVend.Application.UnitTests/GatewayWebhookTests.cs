using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using GjejVend.Infrastructure.Payments;

namespace GjejVend.Application.UnitTests;

public sealed class FakeGatewayWebhookTests
{
    private readonly FakePaymentGateway _gateway = new("test-secret");

    [Fact]
    public void Accepts_a_correctly_signed_success_event()
    {
        const string body = """{"type":"payment.succeeded","reference":"fake_pay_1"}""";

        var result = _gateway.ParseWebhook(body, _gateway.Sign(body));

        Assert.Equal(GatewayEventType.PaymentSucceeded, result?.Type);
        Assert.Equal("fake_pay_1", result?.Reference);
    }

    [Fact]
    public void Accepts_a_failure_event_with_its_reason()
    {
        const string body = """{"type":"payment.failed","reference":"fake_pay_1","reason":"declined"}""";

        var result = _gateway.ParseWebhook(body, _gateway.Sign(body));

        Assert.Equal(GatewayEventType.PaymentFailed, result?.Type);
        Assert.Equal("declined", result?.FailureReason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-signature")]
    public void Rejects_a_missing_or_wrong_signature(string? signature)
        => Assert.Null(_gateway.ParseWebhook("""{"type":"payment.succeeded","reference":"x"}""", signature));

    [Fact]
    public void Rejects_a_body_that_was_changed_after_signing()
    {
        const string signed = """{"type":"payment.failed","reference":"x"}""";
        var signature = _gateway.Sign(signed);

        Assert.Null(_gateway.ParseWebhook("""{"type":"payment.succeeded","reference":"x"}""", signature));
    }

    [Theory]
    [InlineData("""{"type":"payment.refunded","reference":"x"}""")]
    [InlineData("""{"type":"payment.succeeded"}""")]
    [InlineData("""{"type":"payment.succeeded","reference":""}""")]
    [InlineData("not json")]
    public void Rejects_unknown_or_malformed_events_even_when_signed(string body)
        => Assert.Null(_gateway.ParseWebhook(body, _gateway.Sign(body)));

    [Fact]
    public async Task Issues_stable_references_and_invoice_links()
    {
        var checkout = await _gateway.CreatePaymentAsync(new GatewayPaymentRequest(Guid.NewGuid(), Guid.NewGuid(), 5m, "EUR", "test"));
        var invoice = await _gateway.GetInvoiceAsync(checkout.Reference);

        Assert.StartsWith("fake_pay_", checkout.Reference);
        Assert.NotNull(invoice?.HostedUrl);
        Assert.Equal("fake_ref_abc", await _gateway.RefundAsync(checkout.Reference, 1m, "r", "abc"));
    }
}

/// <summary>Stripe events are verified with real Stripe signature rules: t=timestamp,v1=HMAC-SHA256("t.payload").</summary>
public sealed class StripeWebhookTests
{
    private const string Secret = "whsec_unit_test_secret";
    private static readonly Guid PaymentId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    private readonly StripePaymentGateway _gateway = new(
        new PaymentsSettings("sk_test_unit", Secret, "http://localhost:5173", "unused"));

    private static string Event(string type, string dataObject)
        => $$$"""{"id":"evt_1","object":"event","api_version":"2024-06-20","created":1,"livemode":false,"pending_webhooks":1,"request":{"id":null,"idempotency_key":null},"type":"{{{type}}}","data":{"object":{{{dataObject}}}}}""";

    private static string Session(string paymentStatus, string id = "cs_test_1")
        => $$$"""{"id":"{{{id}}}","object":"checkout.session","payment_status":"{{{paymentStatus}}}","metadata":{"payment_id":"{{{PaymentId}}}"}}""";

    private static string Sign(string body, DateTimeOffset? at = null, string secret = Secret)
    {
        var timestamp = (at ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var mac = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{body}"));
        return $"t={timestamp},v1={Convert.ToHexString(mac).ToLowerInvariant()}";
    }

    [Fact]
    public void A_paid_checkout_session_means_the_payment_succeeded()
    {
        var body = Event("checkout.session.completed", Session("paid"));

        var result = _gateway.ParseWebhook(body, Sign(body));

        Assert.Equal(GatewayEventType.PaymentSucceeded, result?.Type);
        Assert.Equal("cs_test_1", result?.Reference);
        Assert.Equal(PaymentId, result?.PaymentId);
    }

    [Fact]
    public void A_completed_session_that_is_not_paid_yet_is_ignored()
    {
        var body = Event("checkout.session.completed", Session("unpaid"));

        Assert.Equal(GatewayEventType.Ignored, _gateway.ParseWebhook(body, Sign(body))?.Type);
    }

    [Fact]
    public void An_expired_checkout_session_means_the_payment_failed()
    {
        var body = Event("checkout.session.expired", Session("unpaid"));

        var result = _gateway.ParseWebhook(body, Sign(body));

        Assert.Equal(GatewayEventType.PaymentFailed, result?.Type);
        Assert.Equal("cs_test_1", result?.Reference);
    }

    [Fact]
    public void A_declined_card_is_a_failure_that_names_our_payment()
    {
        var intent = $$$"""{"id":"pi_1","object":"payment_intent","metadata":{"payment_id":"{{{PaymentId}}}"},"last_payment_error":{"type":"card_error","message":"Your card was declined."}}""";
        var body = Event("payment_intent.payment_failed", intent);

        var result = _gateway.ParseWebhook(body, Sign(body));

        Assert.Equal(GatewayEventType.PaymentFailed, result?.Type);
        Assert.Equal(PaymentId, result?.PaymentId);
        Assert.Equal("Your card was declined.", result?.FailureReason);
    }

    [Fact]
    public void Other_stripe_events_are_acknowledged_and_ignored()
    {
        var body = Event("customer.created", """{"id":"cus_1","object":"customer"}""");

        Assert.Equal(GatewayEventType.Ignored, _gateway.ParseWebhook(body, Sign(body))?.Type);
    }

    [Fact]
    public void Rejects_a_signature_made_with_another_secret()
    {
        var body = Event("checkout.session.completed", Session("paid"));

        Assert.Null(_gateway.ParseWebhook(body, Sign(body, secret: "whsec_someone_else")));
    }

    [Fact]
    public void Rejects_a_body_changed_after_signing()
    {
        var body = Event("checkout.session.completed", Session("paid"));
        var signature = Sign(body);

        Assert.Null(_gateway.ParseWebhook(body.Replace("cs_test_1", "cs_test_2", StringComparison.Ordinal), signature));
    }

    [Fact]
    public void Rejects_an_old_signature_to_stop_replays()
    {
        var body = Event("checkout.session.completed", Session("paid"));

        Assert.Null(_gateway.ParseWebhook(body, Sign(body, DateTimeOffset.UtcNow.AddMinutes(-30))));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("garbage")]
    public void Rejects_a_missing_or_malformed_header(string? header)
        => Assert.Null(_gateway.ParseWebhook(Event("checkout.session.completed", Session("paid")), header));
}
