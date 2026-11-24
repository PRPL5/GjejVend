using GjejVend.Application.Admin;
using GjejVend.Application.Payments;
using GjejVend.Domain.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers;

public sealed record ReviewVerificationRequest(bool Approve, string? Notes);

public sealed record ResolveReportRequest(ReportAction Action, string Resolution);

public sealed record SetCommissionRequest(decimal Rate);

public sealed record SetRefundPolicyRequest(string Policy);

/// <summary>Admin endpoints. Every service call checks the admin role, so the controller stays thin.</summary>
[ApiController]
[Authorize]
[Route("api/admin")]
public sealed class AdminController(
    IVerificationService verifications,
    IReportService reports,
    IAuditLogService auditLogs,
    IAdminSettingsService settings,
    IEarningsService earnings,
    TimeProvider clock) : ControllerBase
{
    [HttpGet("verifications")]
    public async Task<ActionResult<IReadOnlyList<VerificationItem>>> ListVerifications(
        [FromQuery] VerificationRequestStatus? status, CancellationToken ct)
        => Ok(await verifications.ListAsync(status ?? VerificationRequestStatus.Pending, ct));

    [HttpPost("verifications/{id:guid}/review")]
    public async Task<ActionResult<VerificationItem>> ReviewVerification(
        Guid id, [FromBody] ReviewVerificationRequest request, CancellationToken ct)
        => Ok(await verifications.ReviewAsync(id, request.Approve, request.Notes, ct));

    [HttpGet("reports")]
    public async Task<ActionResult<IReadOnlyList<ReportItem>>> ListReports([FromQuery] ReportStatus? status, CancellationToken ct)
        => Ok(await reports.ListAsync(status ?? ReportStatus.Open, ct));

    [HttpPost("reports/{id:guid}/resolve")]
    public async Task<ActionResult<ReportItem>> ResolveReport(Guid id, [FromBody] ResolveReportRequest request, CancellationToken ct)
        => Ok(await reports.ResolveAsync(id, request.Action, request.Resolution, ct));

    [HttpGet("audit-logs")]
    public async Task<ActionResult<PagedResult<AuditLogItem>>> ListAuditLogs(
        [FromQuery] Guid? actorId, [FromQuery] string? entityType, [FromQuery] Guid? entityId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        => Ok(await auditLogs.ListAsync(new AuditLogQuery(actorId, entityType, entityId, from, to, page, pageSize), ct));

    [HttpGet("settings")]
    public async Task<ActionResult<PlatformSettingsView>> GetSettings(CancellationToken ct)
        => Ok(await settings.GetAsync(ct));

    [HttpPut("settings/commission-rate")]
    public async Task<ActionResult<PlatformSettingsView>> SetCommission([FromBody] SetCommissionRequest request, CancellationToken ct)
        => Ok(await settings.SetCommissionRateAsync(request.Rate, ct));

    [HttpPut("settings/refund-policy")]
    public async Task<ActionResult<PlatformSettingsView>> SetRefundPolicy([FromBody] SetRefundPolicyRequest request, CancellationToken ct)
        => Ok(await settings.SetRefundPolicyAsync(request.Policy, ct));

    /// <summary>Platform totals for a period. Defaults to the last 30 days.</summary>
    [HttpGet("commission-summary")]
    public async Task<ActionResult<EarningsSummary>> CommissionSummary(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var end = to ?? clock.GetUtcNow().UtcDateTime;
        return Ok(await earnings.GetPlatformSummaryAsync(from ?? end.AddDays(-30), end, ct));
    }
}

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(IReportService reports) : ControllerBase
{
    /// <summary>Report a lot, a review or a user. Any signed-in user can do this.</summary>
    [HttpPost]
    public async Task<ActionResult<ReportItem>> Create([FromBody] CreateReportRequest request, CancellationToken ct)
        => Ok(await reports.CreateAsync(request, ct));
}
