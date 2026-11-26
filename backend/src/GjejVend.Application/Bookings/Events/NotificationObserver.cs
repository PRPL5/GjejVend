using GjejVend.Domain.Bookings;
using GjejVend.Domain.Engagement;
using GjejVend.Infrastructure.Bookings;

namespace GjejVend.Application.Bookings.Events;

internal sealed class NotificationObserver(INotificationRepository notifications, IBookingRepository bookings) : IBookingEventObserver
{
    public async Task OnEventAsync(BookingEvent bookingEvent, CancellationToken cancellationToken)
    {
        if (Describe(bookingEvent) is not { } description)
        {
            return;
        }

        var recipients = new List<Guid> { bookingEvent.CustomerId };
        if (description.NotifyOwner && await bookings.FindLocationOwnerAsync(bookingEvent.LocationId, cancellationToken) is { } ownerId)
        {
            recipients.Add(ownerId);
        }

        await notifications.AddAsync(
            recipients.Distinct().Select(userId => Create(userId, description.Type, description.Title, bookingEvent)).ToList(),
            cancellationToken);
    }

    internal static Notification Create(Guid userId, string type, string title, BookingEvent bookingEvent) => new()
    {
        UserId = userId,
        Type = type,
        Title = title,
        Body = $"Booking {bookingEvent.Reference}",
        EntityType = "booking",
        EntityId = bookingEvent.BookingId,
    };

    private static (string Type, string Title, bool NotifyOwner)? Describe(BookingEvent bookingEvent) => bookingEvent switch
    {
        BookingConfirmed => ("booking_confirmed", "Booking confirmed", true),
        BookingCancelled => ("booking_cancelled", "Booking cancelled", true),
        BookingExtended => ("booking_extended", "Booking extended", true),
        BookingCheckedIn => ("booking_checked_in", "Checked in", false),
        BookingCompleted => ("booking_completed", "Booking completed", false),
        BookingNoShow => ("booking_no_show", "Booking marked as no-show", true),
        _ => null,
    };
}
