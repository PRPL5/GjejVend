using GjejVend.Application.Common;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Payments;

public sealed record RefundContext(decimal PaidAmount, DateTime BookingStartsAt, DateTime CancelledAt);

public sealed record RefundDecision(decimal Amount, string Reason);

/// <summary>Strategy: decides how much of a payment is refunded when a booking is cancelled.</summary>
public interface IRefundPolicy
{
    string Name { get; }
    RefundDecision Decide(RefundContext context);
}

public sealed class FullRefundPolicy : IRefundPolicy
{
    public const string PolicyName = "full";
    public string Name => PolicyName;

    public RefundDecision Decide(RefundContext context) => new(context.PaidAmount, "Full refund");
}

public sealed class NoRefundPolicy : IRefundPolicy
{
    public const string PolicyName = "none";
    public string Name => PolicyName;

    public RefundDecision Decide(RefundContext context) => new(0m, "No refund");
}

/// <summary>Default: 100% at least 24h before start, 50% at least 2h before, otherwise nothing.</summary>
public sealed class TimeBasedRefundPolicy : IRefundPolicy
{
    public const string PolicyName = "time_based";
    public string Name => PolicyName;

    public RefundDecision Decide(RefundContext context)
    {
        var lead = context.BookingStartsAt - context.CancelledAt;
        if (lead >= TimeSpan.FromHours(24)) return new RefundDecision(context.PaidAmount, "Cancelled 24h or more before start");
        if (lead >= TimeSpan.FromHours(2))
            return new RefundDecision(Math.Round(context.PaidAmount * 0.5m, 2, MidpointRounding.AwayFromZero),
                "Cancelled 2h to 24h before start");
        return new RefundDecision(0m, "Cancelled less than 2h before start");
    }
}

public interface IRefundPolicyResolver
{
    Task<IRefundPolicy> ResolveAsync(CancellationToken ct = default);
}

/// <summary>Picks the active policy by name from the <c>refund_policy</c> platform setting.</summary>
public sealed class RefundPolicyResolver(IApplicationDbContext db, IEnumerable<IRefundPolicy> policies) : IRefundPolicyResolver
{
    public async Task<IRefundPolicy> ResolveAsync(CancellationToken ct = default)
    {
        var setting = await db.PlatformSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == PlatformSetting.RefundPolicyKey, ct);
        var name = setting?.Value ?? TimeBasedRefundPolicy.PolicyName;
        return policies.FirstOrDefault(p => p.Name == name)
               ?? policies.First(p => p.Name == TimeBasedRefundPolicy.PolicyName);
    }
}
