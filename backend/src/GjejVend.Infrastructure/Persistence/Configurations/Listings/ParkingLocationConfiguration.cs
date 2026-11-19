using GjejVend.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Listings;

internal sealed class ParkingLocationConfiguration : IEntityTypeConfiguration<ParkingLocation>
{
    public void Configure(EntityTypeBuilder<ParkingLocation> builder)
    {
        builder.ToTable("parking_locations", table =>
        {
            table.HasCheckConstraint("ck_parking_locations_latitude", "latitude BETWEEN -90 AND 90");
            table.HasCheckConstraint("ck_parking_locations_longitude", "longitude BETWEEN -180 AND 180");
            table.HasCheckConstraint("ck_parking_locations_total_capacity", "total_capacity > 0");
            table.HasCheckConstraint("ck_parking_locations_offline_occupied", "offline_occupied BETWEEN 0 AND total_capacity");
            table.HasCheckConstraint("ck_parking_locations_rating_average", "rating_average BETWEEN 1 AND 5");
        });
        builder.Property(l => l.CountryCode).HasColumnType("char(2)").DefaultsTo("XK");
        builder.Property(l => l.Latitude).HasPrecision(9, 6);
        builder.Property(l => l.Longitude).HasPrecision(9, 6);
        builder.Property(l => l.InventoryMode).DefaultsTo(InventoryMode.Capacity);
        builder.Property(l => l.ListingStatus).DefaultsTo(ListingStatus.Draft);
        builder.Property(l => l.VerificationStatus).DefaultsTo(VerificationStatus.Unverified);
        builder.Property(l => l.Timezone).DefaultsTo("Europe/Belgrade");
        builder.Property(l => l.RatingAverage).HasPrecision(3, 2);
        builder.Property(l => l.OfflineOccupied).DefaultsTo(0);
        builder.Property(l => l.RatingCount).DefaultsTo(0);
        builder.HasTimestamps();
        builder.HasOne<ParkingProvider>().WithMany().HasForeignKey(l => l.ProviderId);
        builder.HasIndex(l => l.Slug).IsUnique();
        builder.HasIndex(l => new { l.City, l.ListingStatus }).HasFilter("deleted_at IS NULL");
    }
}
