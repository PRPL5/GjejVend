using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Listings;

internal sealed class ParkingProviderConfiguration : IEntityTypeConfiguration<ParkingProvider>
{
    public void Configure(EntityTypeBuilder<ParkingProvider> builder)
    {
        builder.ToTable("parking_providers");
        builder.Property(p => p.VerificationStatus).DefaultsTo(VerificationStatus.Unverified);
        builder.Property(p => p.ProviderType).DefaultsTo(ProviderType.Individual);
        builder.HasTimestamps();
        builder.HasOne<Profile>().WithMany().HasForeignKey(p => p.OwnerId);
    }
}
