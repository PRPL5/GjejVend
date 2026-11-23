using GjejVend.Application.Common;
using GjejVend.Application.Payments;
using GjejVend.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services, string connectionString, string paymentWebhookSecret)
    {
        services.AddInfrastructure(connectionString, paymentWebhookSecret);

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AccessGuard>();
        services.AddScoped<IPlatformSettingsReader, PlatformSettingsReader>();

        services.AddScoped<ICommissionService, CommissionService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
        return services;
    }
}
