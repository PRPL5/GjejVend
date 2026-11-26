using GjejVend.Application.Common;

namespace GjejVend.Application.Payments;

public sealed record CommissionBreakdown(decimal Rate, decimal Commission, decimal ProviderAmount);

public interface ICommissionService
{
    Task<decimal> GetRateAsync(CancellationToken ct = default);
    Task<CommissionBreakdown> CalculateAsync(decimal totalAmount, CancellationToken ct = default);
}

/// <summary>
/// Reads the platform commission rate so it can change without a redeploy. Bookings snapshot the rate
/// when they are created, and the database ledger uses that snapshot, so changing the setting never
/// rewrites history.
/// </summary>
public sealed class CommissionService(IPlatformSettingsReader settings) : ICommissionService
{
    public const decimal DefaultRate = 0.10m;

    public async Task<decimal> GetRateAsync(CancellationToken ct = default)
    {
        var rate = await settings.GetDecimalAsync(SettingKeys.CommissionRate, ct);
        return rate is >= 0 and <= 1 ? rate.Value : DefaultRate;
    }

    public async Task<CommissionBreakdown> CalculateAsync(decimal totalAmount, CancellationToken ct = default)
        => Calculate(totalAmount, await GetRateAsync(ct));

    /// <summary>Same rounding as the database: round(total * rate, 2), provider keeps the rest.</summary>
    public static CommissionBreakdown Calculate(decimal totalAmount, decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalAmount);
        if (rate is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be between 0 and 1.");

        var commission = Math.Round(totalAmount * rate, 2, MidpointRounding.AwayFromZero);
        return new CommissionBreakdown(rate, commission, totalAmount - commission);
    }
}
