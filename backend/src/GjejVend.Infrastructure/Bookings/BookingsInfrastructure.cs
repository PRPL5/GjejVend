using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Infrastructure.Bookings;

public static class BookingsInfrastructure
{
    public static IServiceCollection AddBookingsInfrastructure(this IServiceCollection services) =>
        services
            .AddScoped<IBookingRepository, BookingRepository>()
            .AddScoped<IReviewRepository, ReviewRepository>()
            .AddScoped<IFavoriteRepository, FavoriteRepository>()
            .AddScoped<INotificationRepository, NotificationRepository>()
            .AddSingleton<INotificationSender, LoggingNotificationSender>();
}
