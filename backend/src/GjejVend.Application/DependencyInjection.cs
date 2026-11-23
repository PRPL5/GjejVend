using GjejVend.Application.Common;
using GjejVend.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, string connectionString)
    {
        services.AddInfrastructure(connectionString);

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AccessGuard>();
        services.AddScoped<IPlatformSettingsReader, PlatformSettingsReader>();
        return services;
    }
}
