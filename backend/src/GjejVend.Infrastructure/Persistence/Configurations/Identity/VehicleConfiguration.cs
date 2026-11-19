using GjejVend.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Identity;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");
        builder.HasTimestamps();
        builder.HasOne<Profile>().WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(v => new { v.UserId, v.Plate }).IsUnique();
        builder.HasIndex(v => v.UserId).IsUnique().HasFilter("is_default").HasDatabaseName("ux_vehicles_default_per_user");
    }
}
