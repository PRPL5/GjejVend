using GjejVend.Application.Common;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Payments;
using GjejVend.Infrastructure.Payments;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GjejVend.Application.Payments;

public sealed record OnboardingLink(Guid ProviderId, string Url);

public sealed record PayoutDto(
    Guid Id, Guid ProviderId, string ProviderName, decimal Amount, string Currency,
    DateOnly PeriodStart, DateOnly PeriodEnd, string Status, DateTime? PaidAt);

public sealed record PayoutRunItem(Guid ProviderId, string ProviderName, string Outcome, decimal Amount, string? Detail);

public interface IPayoutService
{
    /// <summary>Owner: creates the Stripe Connect account if needed and returns the onboarding link.</summary>
    Task<OnboardingLink> StartOnboardingAsync(Guid? providerId, CancellationToken ct = default);

    /// <summary>Owner: payouts made to the signed-in owner's providers.</summary>
    Task<IReadOnlyList<PayoutDto>> ListMineAsync(CancellationToken ct = default);

    /// <summary>Admin: pays every provider with net earnings in the period. Safe to run twice for the same period.</summary>
    Task<IReadOnlyList<PayoutRunItem>> RunAsync(DateOnly periodStart, DateOnly periodEnd, CancellationToken ct = default);

    /// <summary>Admin: all payouts, newest first.</summary>
    Task<IReadOnlyList<PayoutDto>> ListAsync(PayoutStatus? status, CancellationToken ct = default);
}

/// <summary>
/// Pays providers through Stripe Connect transfers. A payout row is created first, the transfer is sent with
/// the payout id as idempotency key, and only then is the row marked paid. The database writes the ledger
/// entry when the status becomes paid.
/// Limitation: a refund that lands after its period was paid out gives a negative net in a later period. Such
/// periods are skipped, so the provider is not charged back automatically.
/// </summary>
public sealed class PayoutService(
    GjejVendDbContext db,
    IPayoutGateway gateway,
    IEarningsService earnings,
    AccessGuard access,
    PaymentsSettings settings,
    TimeProvider clock,
    ILogger<PayoutService> logger) : IPayoutService
{
    public async Task<OnboardingLink> StartOnboardingAsync(Guid? providerId, CancellationToken ct = default)
    {
        var userId = access.RequireUser();
        var owned = await db.Set<ParkingProvider>().Where(p => p.OwnerId == userId && p.DeletedAt == null).ToListAsync(ct);
        if (owned.Count == 0) throw new ForbiddenException("Only parking providers can set up payouts.");

        var provider = providerId is { } id
            ? owned.FirstOrDefault(p => p.Id == id) ?? throw new NotFoundException("Provider not found.")
            : owned.Count == 1 ? owned[0] : throw new BusinessRuleException("Choose which provider to set up.");

        if (string.IsNullOrEmpty(provider.PaymentAccountId))
        {
            provider.PaymentAccountId = await gateway.CreateConnectedAccountAsync(provider.Id, provider.ContactEmail, ct);
            await db.SaveChangesAsync(ct);
        }

        var baseUrl = settings.FrontendBaseUrl.TrimEnd('/');
        var url = await gateway.CreateOnboardingLinkAsync(provider.PaymentAccountId,
            returnUrl: $"{baseUrl}/owner?payouts=ready", refreshUrl: $"{baseUrl}/owner?payouts=refresh", ct);
        return new OnboardingLink(provider.Id, url);
    }

    public async Task<IReadOnlyList<PayoutDto>> ListMineAsync(CancellationToken ct = default)
    {
        var userId = access.RequireUser();
        return await Query()
            .Where(x => x.Provider.OwnerId == userId)
            .OrderByDescending(x => x.Payout.PeriodStart)
            .Select(x => ToDto(x.Payout, x.Provider.DisplayName))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PayoutDto>> ListAsync(PayoutStatus? status, CancellationToken ct = default)
    {
        await access.RequireAdminAsync(ct);
        return await Query()
            .Where(x => status == null || x.Payout.Status == status)
            .OrderByDescending(x => x.Payout.PeriodStart).ThenByDescending(x => x.Payout.CreatedAt)
            .Select(x => ToDto(x.Payout, x.Provider.DisplayName))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PayoutRunItem>> RunAsync(DateOnly periodStart, DateOnly periodEnd, CancellationToken ct = default)
    {
        await access.RequireAdminAsync(ct);
        if (periodEnd < periodStart) throw new BusinessRuleException("The period end must not be before its start.");
        if (periodEnd >= DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime))
            throw new BusinessRuleException("Only finished days can be paid out. Choose a period that ended before today.");

        var from = periodStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var to = periodEnd.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var providerIds = await db.Set<LedgerEntry>().AsNoTracking()
            .Where(l => l.ProviderId != null && l.CreatedAt >= from && l.CreatedAt < to)
            .Select(l => l.ProviderId!.Value).Distinct().ToListAsync(ct);
        var providers = await db.Set<ParkingProvider>().Where(p => providerIds.Contains(p.Id)).ToListAsync(ct);

        var results = new List<PayoutRunItem>();
        foreach (var provider in providers.OrderBy(p => p.DisplayName))
            results.Add(await PayProviderAsync(provider, periodStart, periodEnd, from, to, ct));
        return results;
    }

    private async Task<PayoutRunItem> PayProviderAsync(
        ParkingProvider provider, DateOnly periodStart, DateOnly periodEnd, DateTime from, DateTime to, CancellationToken ct)
    {
        PayoutRunItem Result(string outcome, decimal amount = 0m, string? detail = null)
            => new(provider.Id, provider.DisplayName, outcome, amount, detail);

        var overlaps = await db.Set<ProviderPayout>().AnyAsync(p => p.ProviderId == provider.Id
            && p.Status != PayoutStatus.Failed && p.PeriodStart <= periodEnd && p.PeriodEnd >= periodStart, ct);
        if (overlaps) return Result("skipped", detail: "A payout already covers this period.");

        if (string.IsNullOrEmpty(provider.PaymentAccountId))
            return Result("skipped", detail: "The provider has not set up payouts yet.");

        var summary = await earnings.GetProviderEarningsAsync(provider.Id, from, to, ct);
        if (summary.Net <= 0) return Result("skipped", detail: "Nothing to pay out for this period.");

        var payout = new ProviderPayout
        {
            Id = Guid.NewGuid(),
            ProviderId = provider.Id,
            Amount = summary.Net,
            Currency = summary.Currency,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Status = PayoutStatus.Pending,
        };
        db.Set<ProviderPayout>().Add(payout);
        await db.SaveChangesAsync(ct);

        try
        {
            payout.ProviderReference = await gateway.TransferAsync(provider.PaymentAccountId, payout.Amount, payout.Currency,
                $"GjejVend payout {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}", payout.Id.ToString(), ct);
            payout.Status = PayoutStatus.Paid;
            payout.PaidAt = clock.GetUtcNow().UtcDateTime;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Transfer failed for provider {ProviderId}", provider.Id);
            payout.Status = PayoutStatus.Failed;
            await db.SaveChangesAsync(ct);
            return Result("failed", payout.Amount, "The transfer was rejected. Try again later.");
        }

        await db.SaveChangesAsync(ct);
        return Result("paid", payout.Amount);
    }

    private IQueryable<PayoutWithProvider> Query()
        => from payout in db.Set<ProviderPayout>().AsNoTracking()
           join provider in db.Set<ParkingProvider>().AsNoTracking() on payout.ProviderId equals provider.Id
           select new PayoutWithProvider(payout, provider);

    private static PayoutDto ToDto(ProviderPayout p, string providerName)
        => new(p.Id, p.ProviderId, providerName, p.Amount, p.Currency, p.PeriodStart, p.PeriodEnd, p.Status.ToString(), p.PaidAt);

    private sealed record PayoutWithProvider(ProviderPayout Payout, ParkingProvider Provider);
}
