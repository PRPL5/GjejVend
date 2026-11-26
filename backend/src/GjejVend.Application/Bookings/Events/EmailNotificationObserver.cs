using GjejVend.Domain.Bookings;
using GjejVend.Infrastructure.Bookings;

namespace GjejVend.Application.Bookings.Events;

internal sealed class EmailNotificationObserver(INotificationRepository notifications, INotificationSender sender) : IBookingEventObserver
{
    public async Task OnEventAsync(BookingEvent bookingEvent, CancellationToken cancellationToken)
    {
        var subject = bookingEvent switch
        {
            BookingConfirmed => $"Your GjejVend booking {bookingEvent.Reference} is confirmed",
            BookingCancelled => $"Your GjejVend booking {bookingEvent.Reference} was cancelled",
            _ => null,
        };

        if (subject is null)
        {
            return;
        }

        var contact = await notifications.FindContactAsync(bookingEvent.CustomerId, cancellationToken);
        if (contact is { Email: { } email, NotifyEmail: true, NotifyBooking: true })
        {
            await sender.SendEmailAsync(email, subject, $"Booking reference: {bookingEvent.Reference}", cancellationToken);
        }
    }
}
