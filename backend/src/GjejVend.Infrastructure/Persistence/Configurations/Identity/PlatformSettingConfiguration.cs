using GjejVend.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Identity;

internal sealed class PlatformSettingConfiguration : IEntityTypeConfiguration<PlatformSetting>
{
    public void Configure(EntityTypeBuilder<PlatformSetting> builder)
    {
        builder.ToTable("platform_settings");
        builder.HasKey(s => s.Key);
        builder.Property(s => s.Value).HasColumnType("jsonb");
        builder.HasCreatedAt(nameof(PlatformSetting.UpdatedAt));
        builder.HasOne<Profile>().WithMany().HasForeignKey(s => s.UpdatedBy);
        builder.HasData(
            new PlatformSetting { Key = "commission_rate", Value = "0.10" },
            new PlatformSetting { Key = "pending_booking_ttl_minutes", Value = "15" });
    }
}
