using GjejVend.Domain.Listings;
using GjejVend.Domain.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Pricing;

internal sealed class ParkingRateConfiguration : IEntityTypeConfiguration<ParkingRate>
{
    public void Configure(EntityTypeBuilder<ParkingRate> builder)
    {
        builder.ToTable("parking_rates", table =>
        {
            table.HasCheckConstraint("ck_parking_rates_amounts", "hourly_rate > 0 AND coalesce(daily_rate, 1) > 0 AND coalesce(monthly_rate, 1) > 0");
            table.HasCheckConstraint("ck_parking_rates_duration", "min_minutes > 0 AND (max_minutes IS NULL OR max_minutes >= min_minutes)");
            table.HasCheckConstraint("ck_parking_rates_validity", "valid_to IS NULL OR valid_to > valid_from");
        });
        builder.Property(r => r.Currency).IsCurrency();
        builder.Property(r => r.MinMinutes).DefaultsTo(60);
        builder.HasCreatedAt(nameof(ParkingRate.ValidFrom));
        builder.HasTimestamps();
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(r => r.LocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ParkingSpace>().WithMany().HasForeignKey(r => r.SpaceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.SpaceId);
    }
}
