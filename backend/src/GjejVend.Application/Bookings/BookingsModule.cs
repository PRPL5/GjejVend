using FluentValidation;
using GjejVend.Application.Bookings.Events;
using GjejVend.Infrastructure.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Application.Bookings;

public static class BookingsModule
{
    public static IServiceCollection AddBookings(this IServiceCollection services) =>
        services
            .AddBookingsInfrastructure()
            .AddValidatorsFromAssemblyContaining<BookingService>(ServiceLifetime.Singleton, includeInternalTypes: true)
            .AddScoped<BookingEventPublisher>()
            .AddScoped<IBookingEventObserver, NotificationObserver>()
            .AddScoped<IBookingEventObserver, ReviewReminderObserver>()
            .AddScoped<IBookingEventObserver, EmailNotificationObserver>()
            .AddScoped<IBookingEventObserver, RefundObserver>()
            .AddScoped<BookingService>()
            .AddScoped<IBookingConfirmation>(provider => provider.GetRequiredService<BookingService>())
            .AddScoped<BookingDeskService>()
            .AddScoped<BookingLifecycleService>()
            .AddScoped<ReviewService>()
            .AddScoped<FavoriteService>()
            .AddScoped<NotificationService>();
}
