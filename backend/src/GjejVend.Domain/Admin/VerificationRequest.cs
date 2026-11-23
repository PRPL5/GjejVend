using GjejVend.Domain.Common;

namespace GjejVend.Domain.Admin;

public enum VerificationStatus { Pending, Approved, Rejected }

public enum VerificationTargetType { Provider, Lot }

public sealed class VerificationRequest : Entity
{
    private VerificationRequest() { }

    public VerificationTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public Guid SubmittedBy { get; private set; }
    public string Documents { get; private set; } = "[]";
    public VerificationStatus Status { get; private set; }
    public string? ReviewNotes { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static VerificationRequest Create(VerificationTargetType type, Guid targetId, Guid submittedBy,
        string documents, DateTime now)
        => new()
        {
            TargetType = type,
            TargetId = targetId,
            SubmittedBy = submittedBy,
            Documents = documents,
            Status = VerificationStatus.Pending,
            CreatedAt = now,
        };

    public void Review(Guid adminId, bool approve, string? notes, DateTime now)
    {
        if (Status != VerificationStatus.Pending) throw new DomainException("Request was already reviewed.");
        if (!approve && string.IsNullOrWhiteSpace(notes))
            throw new DomainException("Rejection requires review notes.");
        Status = approve ? VerificationStatus.Approved : VerificationStatus.Rejected;
        ReviewNotes = notes;
        ReviewedBy = adminId;
        ReviewedAt = now;
    }
}
