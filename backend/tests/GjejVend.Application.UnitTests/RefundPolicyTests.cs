using GjejVend.Application.Payments;

namespace GjejVend.Application.UnitTests;

public sealed class RefundPolicyTests
{
    private static readonly DateTime Start = new(2026, 12, 1, 12, 0, 0, DateTimeKind.Utc);

    private static RefundDecision TimeBased(decimal paid, TimeSpan leadTime)
        => new TimeBasedRefundPolicy().Decide(new RefundContext(paid, Start, Start - leadTime));

    [Theory]
    [InlineData(48, 0, 10.00)] // two days before: everything back
    [InlineData(24, 0, 10.00)] // exactly 24h counts as 24h or more
    [InlineData(23, 59, 5.00)]
    [InlineData(5, 0, 5.00)]
    [InlineData(2, 0, 5.00)] // exactly 2h counts as 2h or more
    [InlineData(1, 59, 0.00)]
    [InlineData(0, 10, 0.00)]
    public void Time_based_policy_refunds_by_how_early_the_booking_is_cancelled(int hours, int minutes, double expected)
        => Assert.Equal((decimal)expected, TimeBased(10m, new TimeSpan(hours, minutes, 0)).Amount);

    [Fact]
    public void Cancelling_after_the_start_refunds_nothing()
        => Assert.Equal(0m, TimeBased(10m, TimeSpan.FromHours(-1)).Amount);

    [Fact]
    public void Half_refund_rounds_away_from_zero_to_cents()
        => Assert.Equal(1.51m, TimeBased(3.01m, TimeSpan.FromHours(5)).Amount);

    [Fact]
    public void Every_decision_explains_itself()
    {
        Assert.False(string.IsNullOrWhiteSpace(TimeBased(10m, TimeSpan.FromHours(48)).Reason));
        Assert.False(string.IsNullOrWhiteSpace(TimeBased(10m, TimeSpan.FromMinutes(5)).Reason));
    }

    [Fact]
    public void Full_policy_refunds_whatever_is_refundable()
        => Assert.Equal(7.5m, new FullRefundPolicy().Decide(new RefundContext(7.5m, Start, Start)).Amount);

    [Fact]
    public void None_policy_refunds_nothing()
        => Assert.Equal(0m, new NoRefundPolicy().Decide(new RefundContext(7.5m, Start, Start - TimeSpan.FromDays(10))).Amount);

    [Theory]
    [InlineData("full", FullRefundPolicy.PolicyName)]
    [InlineData("none", NoRefundPolicy.PolicyName)]
    [InlineData("time_based", TimeBasedRefundPolicy.PolicyName)]
    [InlineData(null, TimeBasedRefundPolicy.PolicyName)] // not configured: default
    [InlineData("something_else", TimeBasedRefundPolicy.PolicyName)] // unknown: default
    public async Task Resolver_picks_the_configured_policy_or_the_default(string? configured, string expected)
    {
        var resolver = new RefundPolicyResolver(
            new FakeSettings(configured), [new TimeBasedRefundPolicy(), new FullRefundPolicy(), new NoRefundPolicy()]);

        Assert.Equal(expected, (await resolver.ResolveAsync()).Name);
    }
}
