using GjejVend.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Listings;

internal sealed class ParkingSpaceConfiguration : IEntityTypeConfiguration<ParkingSpace>
{
    public void Configure(EntityTypeBuilder<ParkingSpace> builder)
    {
        builder.ToTable("parking_spaces", table =>
            table.HasCheckConstraint("ck_parking_spaces_dimensions", "coalesce(length_cm, 1) > 0 AND coalesce(width_cm, 1) > 0 AND coalesce(height_limit_cm, 1) > 0"));
        builder.HasTimestamps();
        builder.Property(s => s.Status).DefaultsTo(SpaceStatus.Active);
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(s => s.LocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(s => new { s.LocationId, s.Label }).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
