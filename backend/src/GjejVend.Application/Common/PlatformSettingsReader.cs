using System.Globalization;
using GjejVend.Domain.Identity;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Common;

public interface IPlatformSettingsReader
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task<decimal?> GetDecimalAsync(string key, CancellationToken ct = default);
}

/// <summary>Reads platform_settings. Values are stored as jsonb, so strings may arrive quoted.</summary>
public sealed class PlatformSettingsReader(GjejVendDbContext db) : IPlatformSettingsReader
{
    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var raw = await db.Set<PlatformSetting>().AsNoTracking()
            .Where(s => s.Key == key).Select(s => s.Value).FirstOrDefaultAsync(ct);
        return raw?.Trim().Trim('"');
    }

    public async Task<decimal?> GetDecimalAsync(string key, CancellationToken ct = default)
        => decimal.TryParse(await GetAsync(key, ct), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
}

public static class SettingKeys
{
    public const string CommissionRate = "commission_rate";
    public const string RefundPolicy = "refund_policy";
}
