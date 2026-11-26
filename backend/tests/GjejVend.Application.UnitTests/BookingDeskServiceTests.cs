using GjejVend.Application.Bookings;
using GjejVend.Application.Bookings.Events;
using GjejVend.Domain.Bookings;
using GjejVend.Domain.Common;
using GjejVend.Infrastructure.Bookings;
using GjejVend.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GjejVend.Application.UnitTests;

public sealed class BookingDeskServiceTests
{
    private readonly IBookingRepository _bookings = Substitute.For<IBookingRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IBookingEventObserver _observer = Substitute.For<IBookingEventObserver>();
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _locationId = Guid.NewGuid();
    private readonly BookingDeskService _desk;

    public BookingDeskServiceTests()
    {
        _currentUser.Id.Returns(_ownerId);
        _bookings.FindLocationOwnerAsync(_locationId, Arg.Any<CancellationToken>()).Returns(_ownerId);
        _desk = new BookingDeskService(
            _bookings,
            new BookingEventPublisher([_observer], NullLogger<BookingEventPublisher>.Instance),
            _currentUser,
            TimeProvider.System,
            new ArrivalsQueryValidator(),
            new TicketScanRequestValidator());
    }

    [Fact]
    public async Task Scanning_a_valid_ticket_checks_the_booking_in()
    {
        var booking = TestBookings.With(BookingStatus.Confirmed, _locationId);
        _bookings.FindByTicketTokenAsync("token", Arg.Any<CancellationToken>()).Returns(booking);
        _bookings.GetArrivalAsync(booking.Id, Arg.Any<CancellationToken>()).Returns(Arrival(booking));

        await _desk.CheckInByTicketAsync(new TicketScanRequest(_locationId, "token"), CancellationToken.None);

        Assert.Equal(BookingStatus.Active, booking.Status);
        await _bookings.Received(1).SaveAsync(booking, Arg.Any<CancellationToken>());
        await _observer.Received(1).OnEventAsync(Arg.Any<BookingCheckedIn>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ticket_for_another_lot_is_rejected()
    {
        var booking = TestBookings.With(BookingStatus.Confirmed);
        _bookings.FindByTicketTokenAsync("token", Arg.Any<CancellationToken>()).Returns(booking);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            _desk.CheckInByTicketAsync(new TicketScanRequest(_locationId, "token"), CancellationToken.None));

        Assert.Equal("ticket_wrong_location", exception.Code);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public async Task Only_the_lot_owner_can_scan()
    {
        _currentUser.Id.Returns(Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _desk.CheckInByTicketAsync(new TicketScanRequest(_locationId, "token"), CancellationToken.None));

        await _bookings.DidNotReceiveWithAnyArgs().FindByTicketTokenAsync(default!, default);
    }

    private static ArrivalView Arrival(Booking booking) => new(
        booking.Id, booking.Reference, booking.Status, booking.LocationId, "Lot", booking.VehiclePlate, booking.VehicleType,
        booking.StartsAt, booking.EndsAt, booking.CheckedInAt, booking.CheckedOutAt);
}
