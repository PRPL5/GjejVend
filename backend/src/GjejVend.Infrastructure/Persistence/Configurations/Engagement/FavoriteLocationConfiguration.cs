using GjejVend.Domain.Engagement;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Engagement;

internal sealed class FavoriteLocationConfiguration : IEntityTypeConfiguration<FavoriteLocation>
{
    public void Configure(EntityTypeBuilder<FavoriteLocation> builder)
    {
        builder.ToTable("favorite_locations");
        builder.HasKey(f => new { f.UserId, f.LocationId });
        builder.HasCreatedAt();
        builder.HasOne<Profile>().WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ParkingLocation>().WithMany().HasForeignKey(f => f.LocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(f => f.LocationId);
    }
}
