using GjejVend.Application.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommissionCalculator, CommissionCalculator>();
        services.AddScoped<ILedgerService, LedgerService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
        return services;
    }
}
