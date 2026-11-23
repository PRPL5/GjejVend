using GjejVend.Application.Common;

namespace GjejVend.Application.Payments;

public sealed record RefundContext(decimal RefundableAmount, DateTime BookingStartsAt, DateTime CancelledAt);

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

    public RefundDecision Decide(RefundContext context) => new(context.RefundableAmount, "Full refund");
}

public sealed class NoRefundPolicy : IRefundPolicy
{
    public const string PolicyName = "none";

    public string Name => PolicyName;

    public RefundDecision Decide(RefundContext context) => new(0m, "No refund");
}

/// <summary>Default policy: 100% at least 24h before start, 50% at least 2h before, otherwise nothing.</summary>
public sealed class TimeBasedRefundPolicy : IRefundPolicy
{
    public const string PolicyName = "time_based";

    public string Name => PolicyName;

    public RefundDecision Decide(RefundContext context)
    {
        var lead = context.BookingStartsAt - context.CancelledAt;

        if (lead >= TimeSpan.FromHours(24))
            return new RefundDecision(context.RefundableAmount, "Cancelled 24 hours or more before start");

        if (lead >= TimeSpan.FromHours(2))
            return new RefundDecision(
                Math.Round(context.RefundableAmount * 0.5m, 2, MidpointRounding.AwayFromZero),
                "Cancelled between 2 and 24 hours before start");

        return new RefundDecision(0m, "Cancelled less than 2 hours before start");
    }
}

public interface IRefundPolicyResolver
{
    Task<IRefundPolicy> ResolveAsync(CancellationToken ct = default);
}

/// <summary>Picks the active policy by name from the refund_policy platform setting.</summary>
public sealed class RefundPolicyResolver(IPlatformSettingsReader settings, IEnumerable<IRefundPolicy> policies)
    : IRefundPolicyResolver
{
    public async Task<IRefundPolicy> ResolveAsync(CancellationToken ct = default)
    {
        var configured = await settings.GetAsync(SettingKeys.RefundPolicy, ct);
        var all = policies.ToList();
        return all.FirstOrDefault(p => p.Name == configured)
               ?? all.First(p => p.Name == TimeBasedRefundPolicy.PolicyName);
    }
}
