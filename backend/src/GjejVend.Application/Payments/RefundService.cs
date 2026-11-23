using GjejVend.Application.Common;
using GjejVend.Domain.Bookings;
using GjejVend.Domain.Payments;
using GjejVend.Infrastructure.Payments;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GjejVend.Application.Payments;

public sealed record RefundPreview(Guid BookingId, string Policy, decimal PaidAmount, decimal RefundAmount, string Currency, string Reason);

public interface IRefundService
{
    /// <summary>What the caller would get back if they cancelled right now.</summary>
    Task<RefundPreview> PreviewAsync(Guid bookingId, CancellationToken ct = default);

    /// <summary>
    /// Refunds a cancelled booking according to the active policy. Called by M2 after it cancels.
    /// Returns null when nothing is refundable (unpaid, policy says zero, or already refunded).
    /// </summary>
    Task<Refund?> RefundCancelledBookingAsync(Guid bookingId, DateTime cancelledAt, CancellationToken ct = default);

    /// <summary>Refunds whatever is still refundable on a payment, ignoring the policy (e.g. paid after expiry).</summary>
    Task<Refund?> RefundInFullAsync(Guid paymentId, string reason, CancellationToken ct = default);
}

/// <summary>
/// Creates refund rows and asks the provider to refund. The database guards the refund amount and, once a
/// refund succeeds, writes the ledger entry and updates the payment status.
/// </summary>
public sealed class RefundService(
    GjejVendDbContext db,
    IPaymentGateway gateway,
    IRefundPolicyResolver policies,
    AccessGuard access,
    TimeProvider clock,
    ILogger<RefundService> logger) : IRefundService
{
    private static readonly PaymentStatus[] RefundableStatuses = [PaymentStatus.Succeeded, PaymentStatus.PartiallyRefunded];

    public async Task<RefundPreview> PreviewAsync(Guid bookingId, CancellationToken ct = default)
    {
        var userId = access.RequireUser();
        var booking = await db.Set<Booking>().AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookingId, ct)
                      ?? throw new NotFoundException("Booking not found.");
        if (booking.CustomerId != userId) throw new ForbiddenException("This booking belongs to another user.");

        var policy = await policies.ResolveAsync(ct);
        var payment = await FindRefundablePaymentAsync(bookingId, ct);
        if (payment is null) return new RefundPreview(bookingId, policy.Name, 0m, 0m, booking.Currency, "Nothing was paid");

        var refundable = await RefundableAmountAsync(payment, ct);
        var decision = policy.Decide(new RefundContext(refundable, booking.StartsAt, clock.GetUtcNow().UtcDateTime));
        return new RefundPreview(bookingId, policy.Name, payment.Amount, Math.Min(decision.Amount, refundable),
            payment.Currency, decision.Reason);
    }

    public async Task<Refund?> RefundCancelledBookingAsync(Guid bookingId, DateTime cancelledAt, CancellationToken ct = default)
    {
        var payment = await FindRefundablePaymentAsync(bookingId, ct);
        if (payment is null) return null; // unpaid: nothing to refund

        // Idempotent: a cancellation processed twice must not refund twice.
        if (await db.Set<Refund>().AnyAsync(r => r.PaymentId == payment.Id && r.Status != RefundStatus.Failed, ct))
            return null;

        var booking = await db.Set<Booking>().AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookingId, ct)
                      ?? throw new NotFoundException("Booking not found.");

        var policy = await policies.ResolveAsync(ct);
        var refundable = await RefundableAmountAsync(payment, ct);
        var decision = policy.Decide(new RefundContext(refundable, booking.StartsAt, cancelledAt));
        var amount = Math.Min(decision.Amount, refundable);
        if (amount <= 0) return null;

        return await IssueRefundAsync(payment, amount, $"{policy.Name}: {decision.Reason}", ct);
    }

    public async Task<Refund?> RefundInFullAsync(Guid paymentId, string reason, CancellationToken ct = default)
    {
        var payment = await db.Set<Payment>().FirstOrDefaultAsync(p => p.Id == paymentId, ct)
                      ?? throw new NotFoundException("Payment not found.");
        if (!RefundableStatuses.Contains(payment.Status)) return null;

        var refundable = await RefundableAmountAsync(payment, ct);
        return refundable <= 0 ? null : await IssueRefundAsync(payment, refundable, reason, ct);
    }

    private async Task<Refund> IssueRefundAsync(Payment payment, decimal amount, string reason, CancellationToken ct)
    {
        var refund = new Refund
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Amount = amount,
            Reason = reason,
            Status = RefundStatus.Pending,
        };
        db.Set<Refund>().Add(refund);

        try
        {
            await db.SaveChangesAsync(ct); // the guard_refund_amount trigger validates the amount here
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { MessageText: var message }
                                           && message.StartsWith("refund_", StringComparison.Ordinal))
        {
            db.Entry(refund).State = EntityState.Detached;
            throw new BusinessRuleException($"The refund was rejected: {message.Replace('_', ' ')}.");
        }

        try
        {
            refund.ProviderReference = await gateway.RefundAsync(payment.ProviderReference ?? string.Empty, amount, reason, ct);
            refund.Status = RefundStatus.Succeeded; // the database writes the ledger entry and updates the payment status
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "The provider refused the refund for payment {PaymentId}", payment.Id);
            refund.Status = RefundStatus.Failed;
        }

        await db.SaveChangesAsync(ct);
        return refund;
    }

    private Task<Payment?> FindRefundablePaymentAsync(Guid bookingId, CancellationToken ct)
        => db.Set<Payment>().Where(p => p.BookingId == bookingId && RefundableStatuses.Contains(p.Status))
            .FirstOrDefaultAsync(ct);

    private async Task<decimal> RefundableAmountAsync(Payment payment, CancellationToken ct)
    {
        var refunded = await db.Set<Refund>()
            .Where(r => r.PaymentId == payment.Id && r.Status != RefundStatus.Failed)
            .SumAsync(r => (decimal?)r.Amount, ct) ?? 0m;
        return payment.Amount - refunded;
    }
}
