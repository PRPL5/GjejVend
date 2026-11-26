using GjejVend.Domain.Bookings;
using GjejVend.Infrastructure.Bookings;

namespace GjejVend.Application.Bookings.Events;

internal sealed class ReviewReminderObserver(INotificationRepository notifications) : IBookingEventObserver
{
    public Task OnEventAsync(BookingEvent bookingEvent, CancellationToken cancellationToken) =>
        bookingEvent is BookingCompleted
            ? notifications.AddAsync(
                [NotificationObserver.Create(bookingEvent.CustomerId, "review_reminder", "How was your parking?", bookingEvent)],
                cancellationToken)
            : Task.CompletedTask;
}
