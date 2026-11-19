using GjejVend.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Identity;

internal sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Status).DefaultsTo(AccountStatus.Active);
        builder.Property(p => p.PreferredLanguage).DefaultsTo(Language.Sq);
        builder.Property(p => p.NotifyEmail).DefaultsTo(true);
        builder.Property(p => p.NotifyPush).DefaultsTo(true);
        builder.Property(p => p.NotifyBooking).DefaultsTo(true);
        builder.HasTimestamps();
    }
}
