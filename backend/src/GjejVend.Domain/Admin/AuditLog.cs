using GjejVend.Domain.Common;

namespace GjejVend.Domain.Admin;

/// <summary>Append-only record of a significant action. ActorId has no foreign key by design.</summary>
public sealed class AuditLog : Entity
{
    private AuditLog() { }

    public Guid ActorId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static AuditLog Create(Guid actorId, string action, string entityType, Guid? entityId,
        string? metadata, DateTime now)
        => new()
        {
            ActorId = actorId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Metadata = metadata,
            CreatedAt = now,
        };
}
