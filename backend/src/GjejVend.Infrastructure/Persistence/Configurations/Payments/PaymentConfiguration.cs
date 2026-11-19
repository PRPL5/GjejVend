using GjejVend.Domain.Bookings;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Payments;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments", table => table.HasCheckConstraint("ck_payments_amount", "amount > 0"));
        builder.Property(p => p.Currency).IsCurrency();
        builder.Property(p => p.Status).DefaultsTo(PaymentStatus.Pending);
        builder.Property(p => p.Metadata).IsJson();
        builder.HasTimestamps();
        builder.HasOne<Booking>().WithMany().HasForeignKey(p => p.BookingId);
        builder.HasOne<Profile>().WithMany().HasForeignKey(p => p.CustomerId);
        builder.HasIndex(p => new { p.Provider, p.ProviderReference }).IsUnique().HasFilter("provider_reference IS NOT NULL");
        builder.HasIndex(p => p.BookingId);
        builder.HasIndex(p => p.CustomerId);
    }
}
