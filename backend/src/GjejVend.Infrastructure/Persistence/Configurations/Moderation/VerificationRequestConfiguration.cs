using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Moderation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Moderation;

internal sealed class VerificationRequestConfiguration : IEntityTypeConfiguration<VerificationRequest>
{
    public void Configure(EntityTypeBuilder<VerificationRequest> builder)
    {
        builder.ToTable("verification_requests", table =>
            table.HasCheckConstraint("ck_verification_requests_single_target", "num_nonnulls(provider_id, location_id) = 1"));
        builder.HasTimestamps();
        builder.Property(v => v.Status).DefaultsTo(VerificationRequestStatus.Pending);
        builder.HasOne<ParkingProvider>().WithMany().HasForeignKey(v => v.ProviderId);
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(v => v.LocationId);
        builder.HasOne<Profile>().WithMany().HasForeignKey(v => v.SubmittedBy);
        builder.HasOne<Profile>().WithMany().HasForeignKey(v => v.ReviewerId);
        builder.HasIndex(v => new { v.Status, v.CreatedAt });
        builder.HasIndex(v => v.ProviderId);
        builder.HasIndex(v => v.LocationId);
        builder.HasIndex(v => v.SubmittedBy);
    }
}
