using GjejVend.Domain.Common;

namespace GjejVend.Domain.Admin;

public enum ReportStatus { Open, Resolved, Dismissed }

public enum ReportTargetType { Lot, Review, User }

public enum ReportCategory { Inaccurate, Abuse, Spam, Safety, Other }

public sealed class Report : Entity
{
    private Report() { }

    public Guid ReportedBy { get; private set; }
    public ReportTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public ReportCategory Category { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public ReportStatus Status { get; private set; }
    public string? Resolution { get; private set; }
    public Guid? ResolvedBy { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Report Create(Guid reportedBy, ReportTargetType targetType, Guid targetId,
        ReportCategory category, string description, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("A description is required.");
        return new Report
        {
            ReportedBy = reportedBy,
            TargetType = targetType,
            TargetId = targetId,
            Category = category,
            Description = description.Trim(),
            Status = ReportStatus.Open,
            CreatedAt = now,
        };
    }

    public void Resolve(Guid adminId, string resolution, bool dismissed, DateTime now)
    {
        if (Status != ReportStatus.Open) throw new DomainException("Report is already closed.");
        Status = dismissed ? ReportStatus.Dismissed : ReportStatus.Resolved;
        Resolution = resolution;
        ResolvedBy = adminId;
        ResolvedAt = now;
    }
}
