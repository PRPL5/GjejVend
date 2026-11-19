using GjejVend.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Listings;

internal sealed class ParkingLocationVehicleTypeConfiguration : IEntityTypeConfiguration<ParkingLocationVehicleType>
{
    public void Configure(EntityTypeBuilder<ParkingLocationVehicleType> builder)
    {
        builder.ToTable("parking_location_vehicle_types");
        builder.HasKey(v => new { v.LocationId, v.VehicleType });
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(v => v.LocationId).OnDelete(DeleteBehavior.Cascade);
    }
}
