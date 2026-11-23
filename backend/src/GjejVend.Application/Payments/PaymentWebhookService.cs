using GjejVend.Domain.Bookings;
using GjejVend.Domain.Payments;
using GjejVend.Infrastructure.Payments;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GjejVend.Application.Payments;

public interface IPaymentWebhookService
{
    /// <summary>Returns false when the signature is invalid. Safe to call repeatedly with the same payload.</summary>
    Task<bool> HandleAsync(string body, string? signature, CancellationToken ct = default);
}

public sealed class PaymentWebhookService(
    GjejVendDbContext db,
    IPaymentGateway gateway,
    TimeProvider clock,
    ILogger<PaymentWebhookService> logger) : IPaymentWebhookService
{
    public async Task<bool> HandleAsync(string body, string? signature, CancellationToken ct = default)
    {
        var webhookEvent = gateway.ParseWebhook(body, signature);
        if (webhookEvent is null) return false;

        var payment = await db.Set<Payment>().AsNoTracking()
            .FirstOrDefaultAsync(p => p.Provider == gateway.Name && p.ProviderReference == webhookEvent.Reference, ct);
        if (payment is null)
        {
            logger.LogWarning("Webhook for unknown payment reference {Reference}", webhookEvent.Reference);
            return true; // acknowledge so the provider stops retrying
        }

        if (webhookEvent.Type == GatewayEventType.PaymentSucceeded) await ConfirmAsync(payment, ct);
        else await FailAsync(payment, ct);
        return true;
    }

    /// <summary>
    /// Marks the payment succeeded and confirms the booking in one transaction. The database writes the
    /// ledger from the payment status change. The conditional update makes duplicate and concurrent
    /// deliveries harmless: only the first one changes the row.
    /// </summary>
    private Task ConfirmAsync(Payment payment, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var now = clock.GetUtcNow().UtcDateTime;

            var updated = await db.Set<Payment>()
                .Where(p => p.Id == payment.Id
                            && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Authorized || p.Status == PaymentStatus.Failed))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, PaymentStatus.Succeeded)
                    .SetProperty(p => p.PaidAt, now), ct);

            if (updated == 0)
            {
                await transaction.CommitAsync(ct); // already processed
                return;
            }

            var confirmed = await db.Set<Booking>()
                .Where(b => b.Id == payment.BookingId && b.Status == BookingStatus.Pending)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, BookingStatus.Confirmed), ct);

            if (confirmed == 0)
            {
                // Paid after the booking expired or was cancelled. The refund flow handles this case.
                logger.LogWarning("Payment {PaymentId} succeeded but booking {BookingId} is no longer pending",
                    payment.Id, payment.BookingId);
            }

            await transaction.CommitAsync(ct);
        });
    }

    private async Task FailAsync(Payment payment, CancellationToken ct)
    {
        // A failed payment leaves the booking pending; the database expires it when the window closes.
        await db.Set<Payment>()
            .Where(p => p.Id == payment.Id && p.Status == PaymentStatus.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, PaymentStatus.Failed), ct);
    }
}
