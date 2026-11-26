using GjejVend.Application.Payments;
using GjejVend.Domain.Bookings;

namespace GjejVend.Application.Bookings.Events;

internal sealed class RefundObserver(IRefundService refunds) : IBookingEventObserver
{
    public Task OnEventAsync(BookingEvent bookingEvent, CancellationToken cancellationToken) =>
        bookingEvent is BookingCancelled cancelled
            ? refunds.RefundCancelledBookingAsync(cancelled.BookingId, cancelled.CancelledAt, cancellationToken)
            : Task.CompletedTask;
}
