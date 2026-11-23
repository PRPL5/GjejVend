using GjejVend.Application.Common;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Payments;

public sealed record EarningsSummary(
    Guid ProviderId,
    DateTime From,
    DateTime To,
    string Currency,
    decimal Gross,
    decimal Commission,
    decimal Refunded,
    decimal Net,
    int PaidBookings);

public interface ILedgerService
{
    Task RecordPaymentAsync(Payment payment, CommissionBreakdown split, CancellationToken ct = default);

    /// <summary>Adds the refund and reverses commission and provider earning in proportion to the refund.</summary>
    Task RecordRefundAsync(Payment payment, Refund refund, CancellationToken ct = default);

    /// <summary>Provider earnings for a period, computed only from ledger rows.</summary>
    Task<EarningsSummary> GetProviderEarningsAsync(Guid providerId, DateTime from, DateTime to, CancellationToken ct = default);
}

/// <summary>Writes append-only ledger rows. Rows are never updated or deleted.</summary>
public sealed class LedgerService(IApplicationDbContext db, IClock clock) : ILedgerService
{
    public Task RecordPaymentAsync(Payment payment, CommissionBreakdown split, CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        db.LedgerEntries.Add(LedgerEntry.Create(LedgerEntryType.Payment, payment.Amount, payment.Currency, now,
            payment.BookingId, payment.Id, providerId: payment.ProviderId));
        db.LedgerEntries.Add(LedgerEntry.Create(LedgerEntryType.Commission, split.Commission, payment.Currency, now,
            payment.BookingId, payment.Id, providerId: payment.ProviderId));
        db.LedgerEntries.Add(LedgerEntry.Create(LedgerEntryType.ProviderEarning, split.ProviderAmount, payment.Currency, now,
            payment.BookingId, payment.Id, providerId: payment.ProviderId));
        return Task.CompletedTask;
    }

    public async Task RecordRefundAsync(Payment payment, Refund refund, CancellationToken ct = default)
    {
        // Use the commission that was actually charged on this payment, not today's rate.
        var charged = await db.LedgerEntries.AsNoTracking()
            .Where(l => l.PaymentId == payment.Id && l.Type == LedgerEntryType.Commission)
            .Select(l => l.Amount).FirstOrDefaultAsync(ct);

        var commissionReversal = Math.Round(refund.Amount * charged / payment.Amount, 2, MidpointRounding.AwayFromZero);
        var earningReversal = refund.Amount - commissionReversal;
        var now = clock.UtcNow;

        db.LedgerEntries.Add(LedgerEntry.Create(LedgerEntryType.Refund, -refund.Amount, payment.Currency, now,
            payment.BookingId, payment.Id, refund.Id, providerId: payment.ProviderId));
        db.LedgerEntries.Add(LedgerEntry.Create(LedgerEntryType.CommissionReversal, -commissionReversal, payment.Currency, now,
            payment.BookingId, payment.Id, refund.Id, providerId: payment.ProviderId));
        db.LedgerEntries.Add(LedgerEntry.Create(LedgerEntryType.EarningReversal, -earningReversal, payment.Currency, now,
            payment.BookingId, payment.Id, refund.Id, providerId: payment.ProviderId));
    }

    public async Task<EarningsSummary> GetProviderEarningsAsync(Guid providerId, DateTime from, DateTime to,
        CancellationToken ct = default)
    {
        var rows = await db.LedgerEntries.AsNoTracking()
            .Where(l => l.ProviderId == providerId && l.CreatedAt >= from && l.CreatedAt < to)
            .Select(l => new { l.Type, l.Amount, l.BookingId })
            .ToListAsync(ct);

        decimal Sum(LedgerEntryType t) => rows.Where(r => r.Type == t).Sum(r => r.Amount);

        var earned = Sum(LedgerEntryType.ProviderEarning);
        var reversed = Sum(LedgerEntryType.EarningReversal); // negative
        var commission = Sum(LedgerEntryType.Commission) + Sum(LedgerEntryType.CommissionReversal);
        var bookings = rows.Where(r => r.Type == LedgerEntryType.Payment && r.BookingId.HasValue)
            .Select(r => r.BookingId).Distinct().Count();

        return new EarningsSummary(providerId, from, to, "EUR",
            Gross: Sum(LedgerEntryType.Payment) + Sum(LedgerEntryType.Refund),
            Commission: commission,
            Refunded: -Sum(LedgerEntryType.Refund),
            Net: earned + reversed,
            PaidBookings: bookings);
    }
}
