using GjejVend.Domain.Bookings;
using GjejVend.Domain.Common;

namespace GjejVend.Domain.UnitTests;

public sealed class BookingStateMachineTests
{
    private static readonly HashSet<(BookingStatus From, BookingStatus To)> AllowedByDatabaseGuard =
    [
        (BookingStatus.Pending, BookingStatus.Confirmed),
        (BookingStatus.Pending, BookingStatus.Cancelled),
        (BookingStatus.Pending, BookingStatus.Expired),
        (BookingStatus.Confirmed, BookingStatus.Active),
        (BookingStatus.Confirmed, BookingStatus.Cancelled),
        (BookingStatus.Confirmed, BookingStatus.NoShow),
        (BookingStatus.Active, BookingStatus.Completed),
    ];

    public static TheoryData<BookingStatus, BookingStatus> AllPairs()
    {
        var data = new TheoryData<BookingStatus, BookingStatus>();
        foreach (var from in Enum.GetValues<BookingStatus>())
        {
            foreach (var to in Enum.GetValues<BookingStatus>())
            {
                data.Add(from, to);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllPairs))]
    public void Transition_table_matches_the_database_guard(BookingStatus from, BookingStatus to) =>
        Assert.Equal(AllowedByDatabaseGuard.Contains((from, to)), BookingTransitions.IsAllowed(from, to));

    [Fact]
    public void Confirm_moves_pending_to_confirmed_and_raises_event()
    {
        var booking = NewBooking(BookingStatus.Pending);
        booking.ExpiresAt = DateTime.UtcNow.AddMinutes(10);

        var bookingEvent = booking.Confirm(DateTime.UtcNow);

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(booking.Id, bookingEvent.BookingId);
    }

    [Fact]
    public void Confirm_is_rejected_after_the_payment_window()
    {
        var booking = NewBooking(BookingStatus.Pending);
        booking.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        var exception = Assert.Throws<ConflictException>(() => booking.Confirm(DateTime.UtcNow));

        Assert.Equal("booking_expired", exception.Code);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public void Cancel_records_actor_reason_and_time()
    {
        var booking = NewBooking(BookingStatus.Confirmed);
        var actor = Guid.NewGuid();
        var at = new DateTime(2026, 11, 25, 10, 0, 0, DateTimeKind.Utc);

        var bookingEvent = booking.Cancel(actor, "plans changed", at);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(actor, booking.CancelledBy);
        Assert.Equal("plans changed", booking.CancellationReason);
        Assert.Equal(at, booking.CancelledAt);
        Assert.Equal(actor, bookingEvent.CancelledBy);
    }

    [Fact]
    public void Check_in_and_check_out_record_times()
    {
        var booking = NewBooking(BookingStatus.Confirmed);
        var checkIn = new DateTime(2026, 11, 25, 9, 0, 0, DateTimeKind.Utc);
        var checkOut = checkIn.AddHours(2);

        Assert.IsType<BookingCheckedIn>(booking.CheckIn(checkIn));
        Assert.IsType<BookingCompleted>(booking.CheckOut(checkOut));

        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.Equal(checkIn, booking.CheckedInAt);
        Assert.Equal(checkOut, booking.CheckedOutAt);
    }

    [Fact]
    public void Confirmed_booking_can_become_no_show()
    {
        var booking = NewBooking(BookingStatus.Confirmed);

        Assert.IsType<BookingNoShow>(booking.MarkNoShow());
        Assert.Equal(BookingStatus.NoShow, booking.Status);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.Cancelled)]
    public void Check_in_is_rejected_unless_confirmed(BookingStatus status)
    {
        var booking = NewBooking(status);

        var exception = Assert.Throws<ConflictException>(() => booking.CheckIn(DateTime.UtcNow));

        Assert.Equal("booking_invalid_transition", exception.Code);
        Assert.Equal(status, booking.Status);
    }

    [Theory]
    [InlineData(BookingStatus.Active)]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.Expired)]
    [InlineData(BookingStatus.NoShow)]
    public void Cancel_is_rejected_after_check_in_or_end(BookingStatus status)
    {
        var booking = NewBooking(status);

        Assert.Throws<ConflictException>(() => booking.Cancel(Guid.NewGuid(), null, DateTime.UtcNow));
        Assert.Null(booking.CancelledBy);
    }

    private static Booking NewBooking(BookingStatus status) => new()
    {
        Id = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        LocationId = Guid.NewGuid(),
        Reference = "GVTEST01",
        VehiclePlate = "01ABC123",
        Status = status,
    };
}
