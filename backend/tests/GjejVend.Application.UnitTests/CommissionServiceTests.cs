using GjejVend.Application.Common;
using GjejVend.Application.Payments;

namespace GjejVend.Application.UnitTests;

internal sealed class FakeSettings(string? value) : IPlatformSettingsReader
{
    public Task<string?> GetAsync(string key, CancellationToken ct = default) => Task.FromResult(value);

    public Task<decimal?> GetDecimalAsync(string key, CancellationToken ct = default)
        => Task.FromResult(decimal.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : (decimal?)null);
}

public sealed class CommissionServiceTests
{
    [Theory]
    [InlineData(10.00, 0.10, 1.00, 9.00)]
    [InlineData(4.50, 0.10, 0.45, 4.05)]
    [InlineData(3.00, 0.085, 0.26, 2.74)]
    [InlineData(0.05, 0.10, 0.01, 0.04)] // 0.005 rounds away from zero, like round() in the database
    [InlineData(10.00, 0, 0, 10.00)]
    [InlineData(10.00, 1, 10.00, 0)]
    public void Splits_the_total_into_commission_and_provider_share(double total, double rate, double commission, double provider)
    {
        var result = CommissionService.Calculate((decimal)total, (decimal)rate);

        Assert.Equal((decimal)commission, result.Commission);
        Assert.Equal((decimal)provider, result.ProviderAmount);
        Assert.Equal((decimal)total, result.Commission + result.ProviderAmount);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Rejects_a_rate_outside_zero_to_one(double rate)
        => Assert.Throws<ArgumentOutOfRangeException>(() => CommissionService.Calculate(10m, (decimal)rate));

    [Fact]
    public void Rejects_a_negative_total()
        => Assert.Throws<ArgumentOutOfRangeException>(() => CommissionService.Calculate(-1m, 0.1m));

    [Theory]
    [InlineData("0.085", 0.085)]
    [InlineData("0.10", 0.10)]
    [InlineData(null, 0.10)] // no setting: default
    [InlineData("1.5", 0.10)] // out of range: default
    [InlineData("abc", 0.10)]
    public async Task Reads_the_rate_from_settings_and_falls_back_to_the_default(string? stored, double expected)
    {
        var service = new CommissionService(new FakeSettings(stored));

        Assert.Equal((decimal)expected, await service.GetRateAsync());
    }
}
