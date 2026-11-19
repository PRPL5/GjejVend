namespace GjejVend.Domain.Moderation;

public sealed class VerificationRequest
{
    public Guid Id { get; set; }
    public Guid? ProviderId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid SubmittedBy { get; set; }
    public IList<string> DocumentPaths { get; init; } = [];
    public VerificationRequestStatus Status { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
