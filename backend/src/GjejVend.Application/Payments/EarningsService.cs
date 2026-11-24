using GjejVend.Application.Common;
using GjejVend.Domain.Bookings;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Payments;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Payments;

public sealed record EarningsSummary(
    DateTime From,
    DateTime To,
    string Currency,
    decimal Gross,
    decimal Refunded,
    decimal Commission,
    decimal Net,
    decimal PaidOut,
    decimal Balance,
    int PaidBookings);

public interface IEarningsService
{
    /// <summary>Earnings of the signed-in owner across all their providers.</summary>
    Task<EarningsSummary> GetOwnerEarningsAsync(DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>Admin: totals across all providers. Commission is what the platform earned.</summary>
    Task<EarningsSummary> GetPlatformSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>Earnings of one provider. No authorization check: for admin-side services such as payouts.</summary>
    Task<EarningsSummary> GetProviderEarningsAsync(Guid providerId, DateTime from, DateTime to, CancellationToken ct = default);
}

/// <summary>
/// Computes earnings from the append-only ledger. A refund ledger row carries the full refunded amount, so
/// the provider's share of it (and the commission that is given back) is derived with the commission rate the
/// booking was created with.
/// </summary>
public sealed class EarningsService(GjejVendDbContext db, AccessGuard access) : IEarningsService
{
    public async Task<EarningsSummary> GetOwnerEarningsAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        if (from >= to) throw new BusinessRuleException("'from' must be before 'to'.");
        var userId = access.RequireUser();

        var providerIds = await db.Set<ParkingProvider>().AsNoTracking()
            .Where(p => p.OwnerId == userId && p.DeletedAt == null)
            .Select(p => p.Id).ToListAsync(ct);
        if (providerIds.Count == 0) throw new ForbiddenException("Only parking providers have earnings.");

        return await ComputeAsync(providerIds, from, to, ct);
    }

    public async Task<EarningsSummary> GetPlatformSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        if (from >= to) throw new BusinessRuleException("'from' must be before 'to'.");
        await access.RequireAdminAsync(ct);
        return await ComputeAsync(null, from, to, ct);
    }

    public Task<EarningsSummary> GetProviderEarningsAsync(Guid providerId, DateTime from, DateTime to, CancellationToken ct = default)
        => ComputeAsync([providerId], from, to, ct);

    private async Task<EarningsSummary> ComputeAsync(IReadOnlyList<Guid>? providerIds, DateTime from, DateTime to, CancellationToken ct)
    {
        var rows = await db.Set<LedgerEntry>().AsNoTracking()
            .Where(l => l.ProviderId != null && (providerIds == null || providerIds.Contains(l.ProviderId.Value))
                        && l.CreatedAt >= from && l.CreatedAt < to)
            .Select(l => new { l.EntryType, l.Amount, l.BookingId })
            .ToListAsync(ct);

        var refundedBookingIds = rows.Where(r => r.EntryType == LedgerEntryType.Refund && r.BookingId != null)
            .Select(r => r.BookingId!.Value).Distinct().ToList();
        var rates = await db.Set<Booking>().AsNoTracking()
            .Where(b => refundedBookingIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.CommissionRate, ct);

        decimal Sum(LedgerEntryType type) => rows.Where(r => r.EntryType == type).Sum(r => r.Amount);

        var refundRows = rows.Where(r => r.EntryType == LedgerEntryType.Refund).ToList();
        var commissionGivenBack = refundRows.Sum(r => Math.Round(r.Amount * RateFor(r.BookingId), 2, MidpointRounding.AwayFromZero));
        var refunded = refundRows.Sum(r => r.Amount);
        var providerShareGivenBack = refunded - commissionGivenBack;

        var net = Sum(LedgerEntryType.ProviderEarning) - providerShareGivenBack;
        var paidOut = Sum(LedgerEntryType.Payout);
        var paidBookings = rows.Where(r => r.EntryType == LedgerEntryType.Charge && r.BookingId != null)
            .Select(r => r.BookingId).Distinct().Count();

        return new EarningsSummary(from, to, "EUR",
            Gross: Sum(LedgerEntryType.Charge),
            Refunded: refunded,
            Commission: Sum(LedgerEntryType.Commission) - commissionGivenBack,
            Net: net,
            PaidOut: paidOut,
            Balance: net - paidOut,
            PaidBookings: paidBookings);

        decimal RateFor(Guid? bookingId) => bookingId is { } id && rates.TryGetValue(id, out var rate) ? rate : 0m;
    }
}
