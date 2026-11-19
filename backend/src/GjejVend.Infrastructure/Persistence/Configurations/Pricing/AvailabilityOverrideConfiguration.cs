using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Pricing;

internal sealed class AvailabilityOverrideConfiguration : IEntityTypeConfiguration<AvailabilityOverride>
{
    public void Configure(EntityTypeBuilder<AvailabilityOverride> builder)
    {
        builder.ToTable("availability_overrides", table =>
        {
            table.HasCheckConstraint("ck_availability_overrides_period", "ends_at > starts_at");
            table.HasCheckConstraint("ck_availability_overrides_capacity", "(kind = 'reduced_capacity') = (capacity IS NOT NULL) AND coalesce(capacity, 0) >= 0");
        });
        builder.HasCreatedAt();
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(e => e.LocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ParkingSpace>().WithMany().HasForeignKey(e => e.SpaceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Profile>().WithMany().HasForeignKey(e => e.CreatedBy);
        builder.HasIndex(e => new { e.LocationId, e.StartsAt });
        builder.HasIndex(e => e.SpaceId);
    }
}
