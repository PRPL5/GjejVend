using GjejVend.Domain.Listings;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Payments;

internal sealed class ProviderPayoutConfiguration : IEntityTypeConfiguration<ProviderPayout>
{
    public void Configure(EntityTypeBuilder<ProviderPayout> builder)
    {
        builder.ToTable("provider_payouts", table =>
        {
            table.HasCheckConstraint("ck_provider_payouts_amount", "amount > 0");
            table.HasCheckConstraint("ck_provider_payouts_period", "period_end >= period_start");
        });
        builder.Property(p => p.Currency).IsCurrency();
        builder.Property(p => p.Status).DefaultsTo(PayoutStatus.Pending);
        builder.HasTimestamps();
        builder.HasOne<ParkingProvider>().WithMany().HasForeignKey(p => p.ProviderId);
        builder.HasIndex(p => new { p.ProviderId, p.PeriodStart });
    }
}
