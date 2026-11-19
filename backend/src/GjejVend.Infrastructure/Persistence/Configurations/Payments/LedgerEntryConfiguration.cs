using GjejVend.Domain.Bookings;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Payments;

internal sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("ledger_entries");
        builder.Property(e => e.Currency).IsCurrency();
        builder.HasCreatedAt();
        builder.HasOne<Booking>().WithMany().HasForeignKey(e => e.BookingId);
        builder.HasOne<Payment>().WithMany().HasForeignKey(e => e.PaymentId);
        builder.HasOne<Refund>().WithMany().HasForeignKey(e => e.RefundId);
        builder.HasOne<ProviderPayout>().WithMany().HasForeignKey(e => e.PayoutId);
        builder.HasOne<ParkingProvider>().WithMany().HasForeignKey(e => e.ProviderId);
        builder.HasIndex(e => e.BookingId);
        builder.HasIndex(e => e.PaymentId);
        builder.HasIndex(e => e.RefundId);
        builder.HasIndex(e => e.PayoutId);
        builder.HasIndex(e => new { e.ProviderId, e.CreatedAt });
    }
}
