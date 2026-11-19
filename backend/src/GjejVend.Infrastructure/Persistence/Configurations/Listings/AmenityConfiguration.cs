using GjejVend.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Listings;

internal sealed class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> builder)
    {
        builder.ToTable("amenities");
        builder.HasKey(a => a.Code);
        builder.HasData(
            new Amenity { Code = "cctv", Name = "CCTV", Icon = "cctv" },
            new Amenity { Code = "guarded", Name = "Guarded", Icon = "shield-check" },
            new Amenity { Code = "covered", Name = "Covered", Icon = "warehouse" },
            new Amenity { Code = "ev_charging", Name = "EV charging", Icon = "plug-zap" },
            new Amenity { Code = "open_24_7", Name = "Open 24/7", Icon = "clock" },
            new Amenity { Code = "accessible", Name = "Accessible parking", Icon = "accessibility" },
            new Amenity { Code = "gated", Name = "Gate access", Icon = "door-closed" });
    }
}
