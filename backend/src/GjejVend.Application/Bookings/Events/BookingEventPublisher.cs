using GjejVend.Domain.Bookings;
using Microsoft.Extensions.Logging;

namespace GjejVend.Application.Bookings.Events;

public interface IBookingEventObserver
{
    Task OnEventAsync(BookingEvent bookingEvent, CancellationToken cancellationToken);
}

public sealed class BookingEventPublisher(IEnumerable<IBookingEventObserver> observers, ILogger<BookingEventPublisher> logger)
{
    public async Task PublishAsync(BookingEvent bookingEvent, CancellationToken cancellationToken)
    {
        foreach (var observer in observers)
        {
            try
            {
                await observer.OnEventAsync(bookingEvent, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "{Observer} failed to handle {Event} for booking {BookingId}", observer.GetType().Name, bookingEvent.GetType().Name, bookingEvent.BookingId);
            }
        }
    }
}
