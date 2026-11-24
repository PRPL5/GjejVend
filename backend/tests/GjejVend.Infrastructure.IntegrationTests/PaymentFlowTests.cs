using GjejVend.Application.Common;
using GjejVend.Application.Payments;
using GjejVend.Domain.Payments;
using GjejVend.Infrastructure.Payments;
using GjejVend.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;

namespace GjejVend.Infrastructure.IntegrationTests;

/// <summary>
/// The M3 services against a real database, so the triggers that write the ledger and guard refunds
/// are part of what is tested. Money maths: 1.50 per hour and a 10% commission.
/// </summary>
[Collection(DatabaseGroup.Name)]
public sealed class PaymentFlowTests(DatabaseFixture database)
{
    private readonly FakePaymentGateway _gateway = new("integration-secret");

    private sealed record TestUser(Guid? Id) : ICurrentUser;

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed record Services(
        PaymentService Pay, PaymentWebhookService Webhook, RefundService Refunds, EarningsService Earnings, PayoutService Payouts);

    private Services Build(GjejVendDbContext db, Guid user, TimeProvider? clock = null)
    {
        clock ??= TimeProvider.System;
        var access = new AccessGuard(new TestUser(user), db);
        var settings = new PlatformSettingsReader(db);
        var policies = new RefundPolicyResolver(settings, [new TimeBasedRefundPolicy(), new FullRefundPolicy(), new NoRefundPolicy()]);
        var refunds = new RefundService(db, _gateway, policies, access, clock, NullLogger<RefundService>.Instance);
        var webhook = new PaymentWebhookService(db, _gateway, refunds, clock, NullLogger<PaymentWebhookService>.Instance);
        var earnings = new EarningsService(db, access);
        var payouts = new PayoutService(db, _gateway, earnings, access,
            new PaymentsSettings(null, null, "http://localhost:5173", "unused"), clock, NullLogger<PayoutService>.Instance);
        return new Services(new PaymentService(db, _gateway, access, clock), webhook, refunds, earnings, payouts);
    }

    /// <summary>Pays for the booking the way a driver does: start the payment, then the provider confirms it.</summary>
    private async Task<PayBookingResult> PayAndConfirmAsync(Services services, Guid booking)
    {
        var checkout = await services.Pay.PayBookingAsync(booking);
        var body = $$"""{"type":"payment.succeeded","reference":"{{checkout.Reference}}"}""";
        Assert.True(await services.Webhook.HandleAsync(body, _gateway.Sign(body)));
        return checkout;
    }

    [Fact]
    public async Task Webhook_confirms_the_booking_and_a_duplicate_changes_nothing()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var booking = await connection.BookAsync(customer, await connection.CreateLocationAsync()); // 3h x 1.50 = 4.50
        await using var db = database.CreateContext(new TestUser(customer));
        var services = Build(db, customer);

        var checkout = await services.Pay.PayBookingAsync(booking);
        var again = await services.Pay.PayBookingAsync(booking);
        var body = $$"""{"type":"payment.succeeded","reference":"{{checkout.Reference}}"}""";
        var accepted = await services.Webhook.HandleAsync(body, _gateway.Sign(body));
        var duplicate = await services.Webhook.HandleAsync(body, _gateway.Sign(body));

        Assert.Equal(4.50m, checkout.Amount);
        Assert.Equal(checkout.PaymentId, again.PaymentId); // a double click reuses the open payment
        Assert.True(accepted);
        Assert.True(duplicate);
        Assert.Equal("confirmed", await connection.ScalarAsync<string>("SELECT status FROM bookings WHERE id = $1", booking));
        Assert.Equal("succeeded", await connection.ScalarAsync<string>("SELECT status FROM payments WHERE id = $1", checkout.PaymentId));
        Assert.Equal(3L, await connection.ScalarAsync<long>("SELECT count(*) FROM ledger_entries WHERE booking_id = $1", booking));
    }

    [Fact]
    public async Task Webhook_with_a_bad_signature_is_rejected_and_changes_nothing()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var booking = await connection.BookAsync(customer, await connection.CreateLocationAsync());
        await using var db = database.CreateContext(new TestUser(customer));
        var services = Build(db, customer);
        var checkout = await services.Pay.PayBookingAsync(booking);
        var body = $$"""{"type":"payment.succeeded","reference":"{{checkout.Reference}}"}""";

        Assert.False(await services.Webhook.HandleAsync(body, "forged"));

        Assert.Equal("pending", await connection.ScalarAsync<string>("SELECT status FROM bookings WHERE id = $1", booking));
        Assert.Equal("pending", await connection.ScalarAsync<string>("SELECT status FROM payments WHERE id = $1", checkout.PaymentId));
    }

    [Fact]
    public async Task Another_user_cannot_pay_for_the_booking()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var stranger = await connection.CreateUserAsync();
        var booking = await connection.BookAsync(customer, await connection.CreateLocationAsync());
        await using var db = database.CreateContext(new TestUser(stranger));

        await Assert.ThrowsAsync<ForbiddenException>(() => Build(db, stranger).Pay.PayBookingAsync(booking));
    }

    [Fact]
    public async Task Cancelling_a_day_ahead_refunds_everything_once()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var booking = await connection.BookAsync(customer, await connection.CreateLocationAsync(), startHours: 30, endHours: 32); // 3.00
        await using var db = database.CreateContext(new TestUser(customer));
        var services = Build(db, customer);
        var checkout = await PayAndConfirmAsync(services, booking);

        var refund = await services.Refunds.RefundCancelledBookingAsync(booking, DateTime.UtcNow);
        var second = await services.Refunds.RefundCancelledBookingAsync(booking, DateTime.UtcNow);

        Assert.Equal(3.00m, refund?.Amount);
        Assert.Equal(RefundStatus.Succeeded, refund?.Status);
        Assert.Null(second); // the same cancellation twice must not refund twice
        Assert.Equal("refunded", await connection.ScalarAsync<string>("SELECT status FROM payments WHERE id = $1", checkout.PaymentId));
        Assert.Equal(3.00m, await connection.ScalarAsync<decimal>(
            "SELECT amount FROM ledger_entries WHERE payment_id = $1 AND entry_type = 'refund'", checkout.PaymentId));
    }

    [Fact]
    public async Task Cancelling_an_hour_before_the_start_refunds_nothing()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var booking = await connection.BookAsync(customer, await connection.CreateLocationAsync(), startHours: 1, endHours: 3);
        await using var db = database.CreateContext(new TestUser(customer));
        var services = Build(db, customer);
        var checkout = await PayAndConfirmAsync(services, booking);

        Assert.Null(await services.Refunds.RefundCancelledBookingAsync(booking, DateTime.UtcNow));

        Assert.Equal("succeeded", await connection.ScalarAsync<string>("SELECT status FROM payments WHERE id = $1", checkout.PaymentId));
    }

    [Fact]
    public async Task Earnings_take_back_the_providers_share_of_a_partial_refund()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var location = await connection.CreateLocationAsync();
        var providerId = await connection.ScalarAsync<Guid>("SELECT provider_id FROM parking_locations WHERE id = $1", location);
        var booking = await connection.BookAsync(customer, location, startHours: 5, endHours: 7); // 3.00 paid
        await using var db = database.CreateContext(new TestUser(customer));
        var services = Build(db, customer);
        await PayAndConfirmAsync(services, booking);

        var refund = await services.Refunds.RefundCancelledBookingAsync(booking, DateTime.UtcNow); // 5h ahead: half back
        var summary = await services.Earnings.GetProviderEarningsAsync(providerId, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

        Assert.Equal(1.50m, refund?.Amount);
        Assert.Equal(3.00m, summary.Gross);
        Assert.Equal(1.50m, summary.Refunded);
        Assert.Equal(0.15m, summary.Commission); // 0.30 charged, 0.15 given back
        Assert.Equal(1.35m, summary.Net); // 2.70 earned, 1.35 given back
        Assert.Equal(1, summary.PaidBookings);
    }

    [Fact]
    public async Task Payout_run_pays_the_provider_once_and_the_database_records_it()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var admin = await connection.CreateUserAsync();
        await connection.ExecuteAsync("INSERT INTO user_roles (user_id, role) VALUES ($1, 'admin')", admin);
        var location = await connection.CreateLocationAsync();
        var providerId = await connection.ScalarAsync<Guid>("SELECT provider_id FROM parking_locations WHERE id = $1", location);
        await connection.ExecuteAsync("UPDATE parking_providers SET payment_account_id = 'acct_test' WHERE id = $1", providerId);
        var booking = await connection.BookAsync(customer, location); // 4.50 paid, 4.05 earned

        await using (var customerDb = database.CreateContext(new TestUser(customer)))
            await PayAndConfirmAsync(Build(customerDb, customer), booking);

        // Only finished days are paid out, so run "two days later" for today's period.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await using var adminDb = database.CreateContext(new TestUser(admin));
        var payouts = Build(adminDb, admin, new FixedTimeProvider(DateTimeOffset.UtcNow.AddDays(2))).Payouts;

        var first = (await payouts.RunAsync(today, today)).Single(item => item.ProviderId == providerId);
        var second = (await payouts.RunAsync(today, today)).Single(item => item.ProviderId == providerId);

        Assert.Equal("paid", first.Outcome);
        Assert.Equal(4.05m, first.Amount);
        Assert.Equal("skipped", second.Outcome); // the same period is never paid twice
        Assert.Equal(4.05m, await connection.ScalarAsync<decimal>(
            "SELECT amount FROM ledger_entries WHERE entry_type = 'payout' AND provider_id = $1", providerId));
    }

    [Fact]
    public async Task Payout_run_is_for_admins_only()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        await using var db = database.CreateContext(new TestUser(customer));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await Assert.ThrowsAsync<ForbiddenException>(() => Build(db, customer).Payouts.RunAsync(today, today));
    }
}
