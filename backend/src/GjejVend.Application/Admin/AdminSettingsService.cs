using System.Globalization;
using System.Text.Json;
using GjejVend.Application.Common;
using GjejVend.Application.Payments;
using GjejVend.Domain.Identity;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Admin;

public sealed record PlatformSettingsView(decimal CommissionRate, string RefundPolicy, IReadOnlyList<string> AvailableRefundPolicies);

public interface IAdminSettingsService
{
    Task<PlatformSettingsView> GetAsync(CancellationToken ct = default);

    /// <summary>Sets the commission for bookings created from now on. Existing bookings keep their own rate.</summary>
    Task<PlatformSettingsView> SetCommissionRateAsync(decimal rate, CancellationToken ct = default);

    Task<PlatformSettingsView> SetRefundPolicyAsync(string policy, CancellationToken ct = default);
}

/// <summary>Admin changes to platform settings. The database audits every change to platform_settings.</summary>
public sealed class AdminSettingsService(
    GjejVendDbContext db,
    AccessGuard access,
    ICommissionService commission,
    IRefundPolicyResolver policyResolver,
    IEnumerable<IRefundPolicy> policies) : IAdminSettingsService
{
    public async Task<PlatformSettingsView> GetAsync(CancellationToken ct = default)
    {
        await access.RequireAdminAsync(ct);
        return await BuildViewAsync(ct);
    }

    public async Task<PlatformSettingsView> SetCommissionRateAsync(decimal rate, CancellationToken ct = default)
    {
        var adminId = await access.RequireAdminAsync(ct);
        if (rate is < 0 or > 1) throw new BusinessRuleException("The commission rate must be between 0 and 1 (for example 0.10 for 10%).");

        // Stored as a JSON number, which is what create_booking() reads.
        await UpsertAsync(SettingKeys.CommissionRate, rate.ToString("0.0000", CultureInfo.InvariantCulture), adminId, ct);
        return await BuildViewAsync(ct);
    }

    public async Task<PlatformSettingsView> SetRefundPolicyAsync(string policy, CancellationToken ct = default)
    {
        var adminId = await access.RequireAdminAsync(ct);
        if (!policies.Any(p => p.Name == policy))
            throw new BusinessRuleException($"Unknown refund policy. Choose one of: {string.Join(", ", policies.Select(p => p.Name))}.");

        await UpsertAsync(SettingKeys.RefundPolicy, JsonSerializer.Serialize(policy), adminId, ct);
        return await BuildViewAsync(ct);
    }

    private async Task UpsertAsync(string key, string jsonValue, Guid adminId, CancellationToken ct)
    {
        var setting = await db.Set<PlatformSetting>().FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
        {
            db.Set<PlatformSetting>().Add(new PlatformSetting { Key = key, Value = jsonValue, UpdatedBy = adminId });
        }
        else
        {
            setting.Value = jsonValue;
            setting.UpdatedBy = adminId;
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task<PlatformSettingsView> BuildViewAsync(CancellationToken ct)
        => new(await commission.GetRateAsync(ct), (await policyResolver.ResolveAsync(ct)).Name,
            policies.Select(p => p.Name).Order().ToList());
}
