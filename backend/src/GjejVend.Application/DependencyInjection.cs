using GjejVend.Application.Common.Events;
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

        // Refund policies (Strategy): the active one is chosen by the refund_policy platform setting.
        services.AddSingleton<IRefundPolicy, TimeBasedRefundPolicy>();
        services.AddSingleton<IRefundPolicy, FullRefundPolicy>();
        services.AddSingleton<IRefundPolicy, NoRefundPolicy>();
        services.AddScoped<IRefundPolicyResolver, RefundPolicyResolver>();
        services.AddScoped<IEventSubscriber<BookingCancelled>, RefundOnCancellationHandler>();
        return services;
    }
}
