namespace GjejVend.Domain.Moderation;

public sealed class Report
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? ReportedUserId { get; set; }
    public Guid? ReviewId { get; set; }
    public ReportCategory Category { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public string? Resolution { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
