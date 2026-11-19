using GjejVend.Domain.Engagement;
using GjejVend.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Engagement;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasCreatedAt();
        builder.HasOne<Profile>().WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(n => new { n.UserId, n.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(n => n.UserId).HasFilter("read_at IS NULL").HasDatabaseName("ix_notifications_unread");
    }
}
