using GjejVend.Application.Bookings.Events;
using GjejVend.Application.Payments;
using GjejVend.Domain.Bookings;
using NSubstitute;

namespace GjejVend.Application.UnitTests;

public sealed class RefundObserverTests
{
    private readonly IRefundService _refunds = Substitute.For<IRefundService>();

    [Fact]
    public async Task Cancellation_asks_payments_to_refund_with_the_cancel_time()
    {
        var cancelled = TestBookings.Cancelled(TestBookings.With(BookingStatus.Cancelled));

        await new RefundObserver(_refunds).OnEventAsync(cancelled, CancellationToken.None);

        await _refunds.Received(1).RefundCancelledBookingAsync(cancelled.BookingId, cancelled.CancelledAt, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Other_events_do_not_refund()
    {
        var booking = TestBookings.With(BookingStatus.Active);

        await new RefundObserver(_refunds).OnEventAsync(booking.CheckOut(DateTime.UtcNow), CancellationToken.None);

        await _refunds.DidNotReceiveWithAnyArgs().RefundCancelledBookingAsync(default, default, default);
    }
}
