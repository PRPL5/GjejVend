using GjejVend.Domain.Bookings;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Reviews;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews", table => table.HasCheckConstraint("ck_reviews_rating", "rating BETWEEN 1 AND 5"));
        builder.Property(r => r.Rating).HasColumnType("smallint");
        builder.Property(r => r.Status).DefaultsTo(ReviewStatus.Published);
        builder.HasTimestamps();
        builder.HasOne<Booking>().WithMany().HasForeignKey(r => r.BookingId);
        builder.HasOne<Profile>().WithMany().HasForeignKey(r => r.CustomerId);
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(r => r.LocationId);
        builder.HasIndex(r => r.BookingId).IsUnique();
        builder.HasIndex(r => r.CustomerId);
        builder.HasIndex(r => new { r.LocationId, r.CreatedAt });
    }
}
