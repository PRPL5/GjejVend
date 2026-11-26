using GjejVend.Application.Bookings;
using GjejVend.Application.Bookings.Events;
using GjejVend.Domain.Bookings;
using GjejVend.Domain.Common;
using GjejVend.Infrastructure.Bookings;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GjejVend.Application.UnitTests;

public sealed class BookingLifecycleServiceTests
{
    private readonly IBookingRepository _bookings = Substitute.For<IBookingRepository>();
    private readonly IBookingEventObserver _observer = Substitute.For<IBookingEventObserver>();

    [Fact]
    public async Task Overdue_active_completes_and_unattended_confirmed_becomes_no_show()
    {
        var active = TestBookings.With(BookingStatus.Active);
        var confirmed = TestBookings.With(BookingStatus.Confirmed);
        _bookings.ListOverdueAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([active, confirmed]);

        var closed = await CreateService().CloseOverdueAsync(CancellationToken.None);

        Assert.Equal(2, closed);
        Assert.Equal(BookingStatus.Completed, active.Status);
        Assert.Equal(BookingStatus.NoShow, confirmed.Status);
        await _observer.Received(1).OnEventAsync(Arg.Any<BookingCompleted>(), Arg.Any<CancellationToken>());
        await _observer.Received(1).OnEventAsync(Arg.Any<BookingNoShow>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_booking_changed_concurrently_is_skipped_and_the_rest_continue()
    {
        var raced = TestBookings.With(BookingStatus.Confirmed);
        var next = TestBookings.With(BookingStatus.Confirmed);
        _bookings.ListOverdueAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([raced, next]);
        _bookings.SaveAsync(raced, Arg.Any<CancellationToken>()).ThrowsAsync(new ConflictException("booking_invalid_transition", "changed"));

        var closed = await CreateService().CloseOverdueAsync(CancellationToken.None);

        Assert.Equal(1, closed);
        _bookings.Received(1).Discard(raced);
        Assert.Equal(BookingStatus.NoShow, next.Status);
    }

    private BookingLifecycleService CreateService() => new(
        _bookings,
        new BookingEventPublisher([_observer], NullLogger<BookingEventPublisher>.Instance),
        TimeProvider.System,
        NullLogger<BookingLifecycleService>.Instance);
}
