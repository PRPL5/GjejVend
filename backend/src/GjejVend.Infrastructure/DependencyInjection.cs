using GjejVend.Application.Common;
using GjejVend.Application.Common.Events;
using GjejVend.Application.Payments;
using GjejVend.Infrastructure.Events;
using GjejVend.Infrastructure.Payments;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connection = config.GetConnectionString("Default");
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connection));
        return services.AddInfrastructureCore(config);
    }

    /// <summary>Everything except the database provider, so tests can plug in their own.</summary>
    public static IServiceCollection AddInfrastructureCore(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventPublisher, InProcessEventPublisher>();

        var paymentOptions = config.GetSection(PaymentOptions.Section).Get<PaymentOptions>() ?? new PaymentOptions();
        services.AddSingleton(paymentOptions);
        services.AddSingleton<FakePaymentGateway>();
        services.AddSingleton<IPaymentGateway>(sp => sp.GetRequiredService<FakePaymentGateway>());

        services.AddScoped<IBookingReader, UnavailableBookingReader>();
        services.AddScoped<ILotModerationService, LoggingLotModerationService>();
        return services;
    }
}
