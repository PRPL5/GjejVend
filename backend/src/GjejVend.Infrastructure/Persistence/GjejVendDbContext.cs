using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace GjejVend.Infrastructure.Persistence;

public sealed class GjejVendDbContext(DbContextOptions<GjejVendDbContext> options) : DbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Remove<CascadeDeleteConvention>();
        configurationBuilder.Properties<decimal>().HavePrecision(10, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GjejVendDbContext).Assembly);
        modelBuilder.ApplyDatabaseConventions();
    }
}
