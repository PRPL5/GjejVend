using GjejVend.Application.Admin;
using GjejVend.Application.Common;
using GjejVend.Application.Identity;
using GjejVend.Application.Listings;
using GjejVend.Application.Payments;
using GjejVend.Infrastructure;
using GjejVend.Infrastructure.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services, string connectionString, PaymentsSettings payments)
    {
        services.AddInfrastructure(connectionString, payments);

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AccessGuard>();
        services.AddScoped<IPlatformSettingsReader, PlatformSettingsReader>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IProviderService, ProviderService>();
        services.AddScoped<IParkingLocationFactory, ParkingLocationFactory>();
        services.AddScoped<IListingService, ListingService>();

        services.AddScoped<ICommissionService, CommissionService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();

        // Refund policies (Strategy). The active one is chosen by the refund_policy platform setting.
        services.AddSingleton<IRefundPolicy, TimeBasedRefundPolicy>();
        services.AddSingleton<IRefundPolicy, FullRefundPolicy>();
        services.AddSingleton<IRefundPolicy, NoRefundPolicy>();
        services.AddScoped<IRefundPolicyResolver, RefundPolicyResolver>();
        services.AddScoped<IRefundService, RefundService>();
        services.AddScoped<IEarningsService, EarningsService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPayoutService, PayoutService>();

        services.AddScoped<IVerificationService, VerificationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAdminSettingsService, AdminSettingsService>();
        return services;
    }
}
