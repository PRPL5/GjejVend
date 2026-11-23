using GjejVend.Application.Common;
using GjejVend.Application.Common.Events;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GjejVend.Application.Payments;

/// <summary>M2 publishes <see cref="BookingCancelled"/>; M3 refunds according to the active policy.</summary>
public sealed class RefundOnCancellationHandler(
    IApplicationDbContext db,
    IBookingReader bookings,
    IRefundPolicyResolver policies,
    IPaymentGateway gateway,
    ILedgerService ledger,
    IClock clock,
    ILogger<RefundOnCancellationHandler> log) : IEventSubscriber<BookingCancelled>
{
    public async Task HandleAsync(BookingCancelled @event, CancellationToken ct)
    {
        var payment = await db.Payments
            .Where(p => p.BookingId == @event.BookingId && p.Status == PaymentStatus.Paid)
            .FirstOrDefaultAsync(ct);
        if (payment is null) return; // unpaid booking: nothing to refund

        // Idempotent: a cancellation delivered twice must not refund twice.
        if (await db.Refunds.AnyAsync(r => r.PaymentId == payment.Id && r.Status != RefundStatus.Failed, ct)) return;

        var booking = await bookings.GetForPaymentAsync(@event.BookingId, ct);
        if (booking is null)
        {
            log.LogWarning("Booking {BookingId} not found while refunding payment {PaymentId}", @event.BookingId, payment.Id);
            return;
        }

        var policy = await policies.ResolveAsync(ct);
        var decision = policy.Decide(new RefundContext(payment.RefundableAmount, booking.StartsAt, @event.CancelledAt));
        var amount = Math.Min(decision.Amount, payment.RefundableAmount);
        if (amount <= 0) return;

        var now = clock.UtcNow;
        var refund = Refund.Create(payment.Id, amount, $"{policy.Name}: {decision.Reason}", now);
        db.Refunds.Add(refund);

        try
        {
            var reference = await gateway.RefundAsync(payment.ProviderReference ?? string.Empty, amount, refund.Reason, ct);
            refund.Complete(reference);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Provider refund failed for payment {PaymentId}", payment.Id);
            refund.Fail();
            await db.SaveChangesAsync(ct);
            return;
        }

        payment.ApplyRefund(amount);
        await ledger.RecordRefundAsync(payment, refund, ct);
        await db.SaveChangesAsync(ct);
    }
}
