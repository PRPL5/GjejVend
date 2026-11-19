using GjejVend.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Listings;

internal sealed class ParkingPhotoConfiguration : IEntityTypeConfiguration<ParkingPhoto>
{
    public void Configure(EntityTypeBuilder<ParkingPhoto> builder)
    {
        builder.ToTable("parking_photos");
        builder.HasCreatedAt();
        builder.Property(p => p.SortOrder).DefaultsTo(0);
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(p => p.LocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => new { p.LocationId, p.SortOrder });
        builder.HasIndex(p => p.LocationId).IsUnique().HasFilter("is_primary").HasDatabaseName("ux_parking_photos_primary_per_location");
    }
}
