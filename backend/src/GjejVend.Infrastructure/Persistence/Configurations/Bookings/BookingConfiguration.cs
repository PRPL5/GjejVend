using GjejVend.Domain.Bookings;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Bookings;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings", table =>
        {
            table.HasCheckConstraint("ck_bookings_period", "ends_at > starts_at");
            table.HasCheckConstraint("ck_bookings_amounts", "total_amount >= 0 AND commission_amount >= 0 AND provider_amount >= 0 AND commission_amount + provider_amount = total_amount");
            table.HasCheckConstraint("ck_bookings_commission_rate", "commission_rate BETWEEN 0 AND 1");
            table.HasCheckConstraint("ck_bookings_pending_expiry", "status <> 'pending' OR expires_at IS NOT NULL");
        });
        builder.Property(b => b.Reference)
            .HasDefaultValueSql("'GV' || upper(substr(md5(gen_random_uuid()::text), 1, 8))")
            .HasSentinel(string.Empty);
        builder.Property(b => b.TicketToken)
            .HasDefaultValueSql("replace(gen_random_uuid()::text, '-', '')")
            .HasSentinel(string.Empty);
        builder.Property(b => b.Currency).IsCurrency();
        builder.Property(b => b.Status).DefaultsTo(BookingStatus.Pending);
        builder.Property(b => b.CommissionRate).HasPrecision(5, 4);
        builder.Property(b => b.PricingSnapshot).IsJson();
        builder.HasTimestamps();
        builder.HasOne<Profile>().WithMany().HasForeignKey(b => b.CustomerId);
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(b => b.LocationId);
        builder.HasOne<ParkingSpace>().WithMany().HasForeignKey(b => b.SpaceId);
        builder.HasOne<Vehicle>().WithMany().HasForeignKey(b => b.VehicleId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Profile>().WithMany().HasForeignKey(b => b.CancelledBy);
        builder.HasIndex(b => b.Reference).IsUnique();
        builder.HasIndex(b => b.TicketToken).IsUnique();
        builder.HasIndex(b => new { b.CustomerId, b.StartsAt });
        builder.HasIndex(b => new { b.LocationId, b.StartsAt });
        builder.HasIndex(b => b.SpaceId);
        builder.HasIndex(b => b.VehicleId);
        builder.HasIndex(b => b.ExpiresAt).HasFilter("status = 'pending'");
    }
}
