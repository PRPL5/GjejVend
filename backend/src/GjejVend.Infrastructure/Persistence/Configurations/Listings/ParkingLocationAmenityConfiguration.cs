using GjejVend.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Listings;

internal sealed class ParkingLocationAmenityConfiguration : IEntityTypeConfiguration<ParkingLocationAmenity>
{
    public void Configure(EntityTypeBuilder<ParkingLocationAmenity> builder)
    {
        builder.ToTable("parking_location_amenities");
        builder.HasKey(a => new { a.LocationId, a.AmenityCode });
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(a => a.LocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Amenity>().WithMany().HasForeignKey(a => a.AmenityCode);
    }
}
