using GjejVend.Application.Common;
using GjejVend.Domain.Engagement;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Moderation;
using GjejVend.Domain.Reviews;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Admin;

public sealed record CreateReportRequest(
    Guid? LocationId, Guid? ReportedUserId, Guid? ReviewId, ReportCategory Category, string Description);

/// <summary>What an admin decides for an open report.</summary>
public enum ReportAction
{
    Dismiss,
    Resolve,
    HideReview,
    SuspendLot,
}

public sealed record ReportItem(
    Guid Id, Guid ReporterId, Guid? LocationId, Guid? ReportedUserId, Guid? ReviewId, string Category,
    string? Description, string Status, string? Resolution, DateTime CreatedAt, DateTime? ResolvedAt);

public interface IReportService
{
    /// <summary>Any signed-in user can report a lot, a review or another user.</summary>
    Task<ReportItem> CreateAsync(CreateReportRequest request, CancellationToken ct = default);

    /// <summary>Admin: the report queue, oldest first.</summary>
    Task<IReadOnlyList<ReportItem>> ListAsync(ReportStatus? status, CancellationToken ct = default);

    /// <summary>Admin: closes a report, optionally hiding the review or suspending the lot it points at.</summary>
    Task<ReportItem> ResolveAsync(Guid reportId, ReportAction action, string resolution, CancellationToken ct = default);
}

public sealed class ReportService(GjejVendDbContext db, AccessGuard access, TimeProvider clock) : IReportService
{
    public async Task<ReportItem> CreateAsync(CreateReportRequest request, CancellationToken ct = default)
    {
        var reporterId = access.RequireUser();

        var targets = new[] { request.LocationId, request.ReportedUserId, request.ReviewId }.Count(t => t != null);
        if (targets != 1) throw new BusinessRuleException("Report exactly one thing: a lot, a review or a user.");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new BusinessRuleException("Describe what is wrong.");
        if (request.ReportedUserId == reporterId) throw new BusinessRuleException("You cannot report yourself.");

        var exists = request switch
        {
            { LocationId: { } id } => await db.Set<ParkingLocation>().AnyAsync(l => l.Id == id, ct),
            { ReportedUserId: { } id } => await db.Set<Profile>().AnyAsync(p => p.Id == id, ct),
            { ReviewId: { } id } => await db.Set<Review>().AnyAsync(r => r.Id == id, ct),
            _ => false,
        };
        if (!exists) throw new NotFoundException("The thing you are reporting no longer exists.");

        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = reporterId,
            LocationId = request.LocationId,
            ReportedUserId = request.ReportedUserId,
            ReviewId = request.ReviewId,
            Category = request.Category,
            Description = request.Description.Trim(),
            Status = ReportStatus.Open,
        };
        db.Set<Report>().Add(report);
        await db.SaveChangesAsync(ct);
        return ToItem(report);
    }

    public async Task<IReadOnlyList<ReportItem>> ListAsync(ReportStatus? status, CancellationToken ct = default)
    {
        await access.RequireAdminAsync(ct);
        var reports = await db.Set<Report>().AsNoTracking()
            .Where(r => status == null || r.Status == status)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);
        return reports.Select(ToItem).ToList();
    }

    public async Task<ReportItem> ResolveAsync(Guid reportId, ReportAction action, string resolution, CancellationToken ct = default)
    {
        var adminId = await access.RequireAdminAsync(ct);
        if (string.IsNullOrWhiteSpace(resolution)) throw new BusinessRuleException("Write a short resolution note.");

        var report = await db.Set<Report>().FirstOrDefaultAsync(r => r.Id == reportId, ct)
                     ?? throw new NotFoundException("Report not found.");
        if (report.Status is ReportStatus.Resolved or ReportStatus.Dismissed)
            throw new BusinessRuleException("This report is already closed.");

        switch (action)
        {
            case ReportAction.HideReview:
                var reviewId = report.ReviewId ?? throw new BusinessRuleException("This report is not about a review.");
                var review = await db.Set<Review>().FirstOrDefaultAsync(r => r.Id == reviewId, ct)
                             ?? throw new NotFoundException("Review not found.");
                review.Status = ReviewStatus.Hidden;
                break;

            case ReportAction.SuspendLot:
                var locationId = report.LocationId ?? throw new BusinessRuleException("This report is not about a lot.");
                var location = await db.Set<ParkingLocation>().FirstOrDefaultAsync(l => l.Id == locationId, ct)
                               ?? throw new NotFoundException("Parking lot not found.");
                location.ListingStatus = ListingStatus.Suspended;
                break;
        }

        report.Status = action == ReportAction.Dismiss ? ReportStatus.Dismissed : ReportStatus.Resolved;
        report.Resolution = resolution.Trim();
        report.ResolvedBy = adminId;
        report.ResolvedAt = clock.GetUtcNow().UtcDateTime;

        db.Set<Notification>().Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = report.ReporterId,
            Type = "report_resolved",
            Title = "Your report was reviewed",
            Body = report.Resolution,
            EntityType = "report",
            EntityId = report.Id,
        });

        await db.SaveChangesAsync(ct);
        return ToItem(report);
    }

    private static ReportItem ToItem(Report r)
        => new(r.Id, r.ReporterId, r.LocationId, r.ReportedUserId, r.ReviewId, r.Category.ToString(),
            r.Description, r.Status.ToString(), r.Resolution, r.CreatedAt, r.ResolvedAt);
}
