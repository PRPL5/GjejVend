using GjejVend.Application.Bookings.Events;
using GjejVend.Domain.Bookings;
using GjejVend.Domain.Engagement;
using GjejVend.Infrastructure.Bookings;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GjejVend.Application.UnitTests;

public sealed class BookingEventObserverTests
{
    private readonly INotificationRepository _notifications = Substitute.For<INotificationRepository>();
    private readonly IBookingRepository _bookings = Substitute.For<IBookingRepository>();
    private readonly INotificationSender _sender = Substitute.For<INotificationSender>();

    [Fact]
    public async Task Publisher_notifies_every_observer_even_when_one_fails()
    {
        var failing = Substitute.For<IBookingEventObserver>();
        var healthy = Substitute.For<IBookingEventObserver>();
        var bookingEvent = TestBookings.Cancelled(TestBookings.With(BookingStatus.Cancelled));
        failing.OnEventAsync(bookingEvent, Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));
        var publisher = new BookingEventPublisher([failing, healthy], NullLogger<BookingEventPublisher>.Instance);

        await publisher.PublishAsync(bookingEvent, CancellationToken.None);

        await healthy.Received(1).OnEventAsync(bookingEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancellation_notifies_customer_and_owner()
    {
        var booking = TestBookings.With(BookingStatus.Cancelled);
        var ownerId = Guid.NewGuid();
        _bookings.FindLocationOwnerAsync(booking.LocationId, Arg.Any<CancellationToken>()).Returns(ownerId);
        IReadOnlyCollection<Notification>? written = null;
        await _notifications.AddAsync(Arg.Do<IReadOnlyCollection<Notification>>(n => written = n), Arg.Any<CancellationToken>());

        await new NotificationObserver(_notifications, _bookings).OnEventAsync(TestBookings.Cancelled(booking), CancellationToken.None);

        Assert.NotNull(written);
        Assert.Equal([booking.CustomerId, ownerId], written.Select(n => n.UserId));
        Assert.All(written, n =>
        {
            Assert.Equal("booking_cancelled", n.Type);
            Assert.Equal(booking.Id, n.EntityId);
        });
    }

    [Fact]
    public async Task Booking_created_writes_no_notification()
    {
        var booking = TestBookings.With(BookingStatus.Pending);

        await new NotificationObserver(_notifications, _bookings).OnEventAsync(booking.Created(), CancellationToken.None);

        await _notifications.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Completed_booking_reminds_customer_to_review()
    {
        var booking = TestBookings.With(BookingStatus.Active);
        IReadOnlyCollection<Notification>? written = null;
        await _notifications.AddAsync(Arg.Do<IReadOnlyCollection<Notification>>(n => written = n), Arg.Any<CancellationToken>());

        await new ReviewReminderObserver(_notifications).OnEventAsync(booking.CheckOut(DateTime.UtcNow), CancellationToken.None);

        var reminder = Assert.Single(written!);
        Assert.Equal("review_reminder", reminder.Type);
        Assert.Equal(booking.CustomerId, reminder.UserId);
    }

    [Theory]
    [InlineData(true, true, 1)]
    [InlineData(false, true, 0)]
    [InlineData(true, false, 0)]
    public async Task Confirmation_email_respects_profile_settings(bool notifyEmail, bool notifyBooking, int expectedEmails)
    {
        var booking = TestBookings.With(BookingStatus.Pending);
        _notifications.FindContactAsync(booking.CustomerId, Arg.Any<CancellationToken>())
            .Returns(new NotificationContact("driver@example.test", notifyEmail, notifyBooking));

        await new EmailNotificationObserver(_notifications, _sender).OnEventAsync(booking.Confirm(DateTime.UtcNow), CancellationToken.None);

        await _sender.Received(expectedEmails).SendEmailAsync("driver@example.test", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
