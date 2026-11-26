using GjejVend.Application.Bookings.Events;
using GjejVend.Domain.Bookings;
using GjejVend.Domain.Common;
using GjejVend.Infrastructure.Bookings;
using Microsoft.Extensions.Logging;

namespace GjejVend.Application.Bookings;

public sealed class BookingLifecycleService(
    IBookingRepository bookings,
    BookingEventPublisher events,
    TimeProvider time,
    ILogger<BookingLifecycleService> logger)
{
    private const int BatchSize = 100;

    public async Task<int> CloseOverdueAsync(CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var closed = 0;

        foreach (var booking in await bookings.ListOverdueAsync(now, BatchSize, cancellationToken))
        {
            try
            {
                BookingEvent bookingEvent = booking.Status == BookingStatus.Active ? booking.CheckOut(now) : booking.MarkNoShow();
                await bookings.SaveAsync(booking, cancellationToken);
                await events.PublishAsync(bookingEvent, cancellationToken);
                closed++;
            }
            catch (ConflictException exception)
            {
                bookings.Discard(booking);
                logger.LogWarning(exception, "Booking {BookingId} could not be closed", booking.Id);
            }
        }

        return closed;
    }
}
