using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<ActorConnectionInterceptor>();
        services.AddDbContext<GjejVendDbContext>((provider, options) => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .EnableRetryOnFailure()
                .MigrationsHistoryTable("__EFMigrationsHistory", "private"))
            .AddInterceptors(provider.GetRequiredService<ActorConnectionInterceptor>()));

        services.AddHealthChecks().AddNpgSql(connectionString, name: "database");

        return services;
    }
}
