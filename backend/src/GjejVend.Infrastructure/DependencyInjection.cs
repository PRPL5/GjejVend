using GjejVend.Infrastructure.Identity;
using GjejVend.Infrastructure.Listings;
using GjejVend.Infrastructure.Payments;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, PaymentsSettings payments)
    {
        services.AddScoped<ActorConnectionInterceptor>();
        services.AddDbContext<GjejVendDbContext>((provider, options) => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .EnableRetryOnFailure()
                .MigrationsHistoryTable("__EFMigrationsHistory", "private"))
            .AddInterceptors(provider.GetRequiredService<ActorConnectionInterceptor>()));

        services.AddSingleton(payments);
        if (payments.UseStripe)
        {
            services.AddSingleton<StripePaymentGateway>();
            services.AddSingleton<IPaymentGateway>(sp => sp.GetRequiredService<StripePaymentGateway>());
            services.AddSingleton<IPayoutGateway>(sp => sp.GetRequiredService<StripePaymentGateway>());
        }
        else
        {
            services.AddSingleton(new FakePaymentGateway(payments.FakeWebhookSecret));
            services.AddSingleton<IPaymentGateway>(sp => sp.GetRequiredService<FakePaymentGateway>());
            services.AddSingleton<IPayoutGateway>(sp => sp.GetRequiredService<FakePaymentGateway>());
        }

        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();

        services.AddHealthChecks().AddNpgSql(connectionString, name: "database");

        return services;
    }
}
