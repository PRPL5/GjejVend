using GjejVend.Domain.Moderation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Moderation;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.Property(a => a.Metadata).IsJson();
        builder.HasCreatedAt();
        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.CreatedAt });
        builder.HasIndex(a => new { a.ActorId, a.CreatedAt });
    }
}
