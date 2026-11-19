using GjejVend.Domain.Bookings;
using GjejVend.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Bookings;

internal sealed class BookingStatusChangeConfiguration : IEntityTypeConfiguration<BookingStatusChange>
{
    public void Configure(EntityTypeBuilder<BookingStatusChange> builder)
    {
        builder.ToTable("booking_status_history");
        builder.HasCreatedAt(nameof(BookingStatusChange.ChangedAt));
        builder.HasOne<Booking>().WithMany().HasForeignKey(c => c.BookingId);
        builder.HasOne<Profile>().WithMany().HasForeignKey(c => c.ChangedBy);
        builder.HasIndex(c => new { c.BookingId, c.ChangedAt });
    }
}
