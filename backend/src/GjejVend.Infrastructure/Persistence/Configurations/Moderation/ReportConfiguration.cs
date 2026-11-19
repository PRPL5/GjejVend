using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Moderation;
using GjejVend.Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Moderation;

internal sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports", table =>
            table.HasCheckConstraint("ck_reports_single_target", "num_nonnulls(location_id, reported_user_id, review_id) = 1"));
        builder.HasTimestamps();
        builder.Property(r => r.Status).DefaultsTo(ReportStatus.Open);
        builder.HasOne<Profile>().WithMany().HasForeignKey(r => r.ReporterId);
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(r => r.LocationId);
        builder.HasOne<Profile>().WithMany().HasForeignKey(r => r.ReportedUserId);
        builder.HasOne<Review>().WithMany().HasForeignKey(r => r.ReviewId);
        builder.HasOne<Profile>().WithMany().HasForeignKey(r => r.ResolvedBy);
        builder.HasIndex(r => new { r.Status, r.CreatedAt });
        builder.HasIndex(r => r.ReporterId);
        builder.HasIndex(r => r.LocationId);
        builder.HasIndex(r => r.ReportedUserId);
        builder.HasIndex(r => r.ReviewId);
    }
}
