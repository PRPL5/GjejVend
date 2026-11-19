using GjejVend.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, string connectionString) =>
        services.AddInfrastructure(connectionString);
}
