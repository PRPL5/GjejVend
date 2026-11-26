using FluentValidation;
using GjejVend.Application.Bookings.Events;
using GjejVend.Domain.Bookings;
using GjejVend.Domain.Common;
using GjejVend.Infrastructure.Bookings;
using GjejVend.Infrastructure.Persistence;

namespace GjejVend.Application.Bookings;

public sealed class BookingDeskService(
    IBookingRepository bookings,
    BookingEventPublisher events,
    ICurrentUser currentUser,
    TimeProvider time,
    IValidator<ArrivalsQuery> arrivalsValidator,
    IValidator<TicketScanRequest> scanValidator)
{
    public async Task<IReadOnlyList<ArrivalView>> ListArrivalsAsync(ArrivalsQuery query, CancellationToken cancellationToken)
    {
        await arrivalsValidator.ValidateAndThrowAsync(query, cancellationToken);
        return await bookings.ListArrivalsAsync(currentUser.RequireId(), query.From.UtcDateTime, query.To.UtcDateTime, cancellationToken);
    }

    public async Task<ArrivalView> CheckInAsync(Guid bookingId, CancellationToken cancellationToken) =>
        await ApplyAsync(await bookings.FindAsync(bookingId, cancellationToken), (booking, at) => booking.CheckIn(at), cancellationToken);

    public async Task<ArrivalView> CheckOutAsync(Guid bookingId, CancellationToken cancellationToken) =>
        await ApplyAsync(await bookings.FindAsync(bookingId, cancellationToken), (booking, at) => booking.CheckOut(at), cancellationToken);

    public async Task<ArrivalView> CheckInByTicketAsync(TicketScanRequest request, CancellationToken cancellationToken) =>
        await ApplyAsync(await FindScannedAsync(request, cancellationToken), (booking, at) => booking.CheckIn(at), cancellationToken);

    public async Task<ArrivalView> CheckOutByTicketAsync(TicketScanRequest request, CancellationToken cancellationToken) =>
        await ApplyAsync(await FindScannedAsync(request, cancellationToken), (booking, at) => booking.CheckOut(at), cancellationToken);

    private async Task<Booking> FindScannedAsync(TicketScanRequest request, CancellationToken cancellationToken)
    {
        await scanValidator.ValidateAndThrowAsync(request, cancellationToken);
        await EnsureOwnerAsync(request.LocationId, cancellationToken);

        var booking = await bookings.FindByTicketTokenAsync(request.Token.Trim(), cancellationToken)
            ?? throw new NotFoundException("ticket_not_found", "This ticket does not exist.");

        return booking.LocationId == request.LocationId
            ? booking
            : throw new ConflictException("ticket_wrong_location", "This ticket is for another parking.");
    }

    private async Task<ArrivalView> ApplyAsync(Booking? booking, Func<Booking, DateTime, BookingEvent> change, CancellationToken cancellationToken)
    {
        if (booking is null)
        {
            throw BookingService.BookingNotFound();
        }

        await EnsureOwnerAsync(booking.LocationId, cancellationToken);
        var bookingEvent = change(booking, time.GetUtcNow().UtcDateTime);
        await bookings.SaveAsync(booking, cancellationToken);
        await events.PublishAsync(bookingEvent, cancellationToken);

        return await bookings.GetArrivalAsync(booking.Id, cancellationToken) ?? throw BookingService.BookingNotFound();
    }

    private async Task EnsureOwnerAsync(Guid locationId, CancellationToken cancellationToken)
    {
        if (await bookings.FindLocationOwnerAsync(locationId, cancellationToken) != currentUser.RequireId())
        {
            throw new ForbiddenException("not_location_owner", "Only the owner of this parking can do this.");
        }
    }
}
