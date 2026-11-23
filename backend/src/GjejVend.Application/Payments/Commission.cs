using GjejVend.Application.Common;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Payments;

public sealed record CommissionBreakdown(decimal Rate, decimal Commission, decimal ProviderAmount);

public interface ICommissionCalculator
{
    Task<decimal> GetRateAsync(CancellationToken ct = default);
    Task<CommissionBreakdown> CalculateAsync(decimal totalAmount, CancellationToken ct = default);
}

/// <summary>Reads the commission rate from platform settings so it can change without a redeploy.</summary>
public sealed class CommissionCalculator(IApplicationDbContext db) : ICommissionCalculator
{
    public const decimal DefaultRate = 0.10m;

    public async Task<decimal> GetRateAsync(CancellationToken ct = default)
    {
        var setting = await db.PlatformSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == PlatformSetting.CommissionRateKey, ct);
        return setting is not null && TryParseRate(setting.Value, out var rate) ? rate : DefaultRate;
    }

    public async Task<CommissionBreakdown> CalculateAsync(decimal totalAmount, CancellationToken ct = default)
        => Calculate(totalAmount, await GetRateAsync(ct));

    public static CommissionBreakdown Calculate(decimal totalAmount, decimal rate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalAmount);
        if (rate is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(rate));
        var commission = Math.Round(totalAmount * rate, 2, MidpointRounding.AwayFromZero);
        return new CommissionBreakdown(rate, commission, totalAmount - commission);
    }

    public static bool TryParseRate(string value, out decimal rate)
        => decimal.TryParse(value, System.Globalization.NumberStyles.Number,
               System.Globalization.CultureInfo.InvariantCulture, out rate) && rate is >= 0 and <= 1;
}
