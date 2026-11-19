using GjejVend.Domain.Listings;
using GjejVend.Domain.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Pricing;

internal sealed class OperatingHoursConfiguration : IEntityTypeConfiguration<OperatingHours>
{
    public void Configure(EntityTypeBuilder<OperatingHours> builder)
    {
        builder.ToTable("parking_operating_hours", table =>
            table.HasCheckConstraint("ck_parking_operating_hours_weekday", "weekday BETWEEN 1 AND 7"));
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(h => h.LocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(h => new { h.LocationId, h.Weekday }).IsUnique();
    }
}
