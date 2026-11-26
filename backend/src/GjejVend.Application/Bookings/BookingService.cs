using FluentValidation;
using GjejVend.Application.Bookings.Events;
using GjejVend.Application.Common;
using GjejVend.Application.Listings;
using GjejVend.Domain.Common;
using GjejVend.Infrastructure.Bookings;
using GjejVend.Infrastructure.Persistence;

namespace GjejVend.Application.Bookings;

public interface IBookingConfirmation
{
    Task ConfirmAsync(Guid bookingId, CancellationToken cancellationToken);
}

public sealed class BookingService(
    IBookingRepository bookings,
    IAvailabilityService availability,
    IPricingQuery pricing,
    BookingEventPublisher events,
    ICurrentUser currentUser,
    TimeProvider time,
    IValidator<CheckoutPreviewQuery> previewValidator,
    IValidator<CreateBookingRequest> createValidator,
    IValidator<ExtendBookingRequest> extendValidator,
    IValidator<CancelBookingRequest> cancelValidator) : IBookingConfirmation
{
    public async Task<CheckoutPreview> PreviewAsync(CheckoutPreviewQuery query, CancellationToken cancellationToken)
    {
        await previewValidator.ValidateAndThrowAsync(query, cancellationToken);
        var location = await bookings.FindBookableLocationAsync(query.LocationId, cancellationToken) ?? throw LocationNotFound();
        var (startsAt, endsAt) = (query.StartsAt.UtcDateTime, query.EndsAt.UtcDateTime);

        var isOpen = await availability.IsOpenAsync(location.Id, startsAt, endsAt, cancellationToken);
        var freeCapacity = await availability.GetFreeCapacityAsync(location.Id, startsAt, endsAt, cancellationToken);
        var quote = await pricing.QuoteAsync(location.Id, null, startsAt, endsAt, cancellationToken);
        var vehicle = await bookings.FindDefaultVehicleAsync(currentUser.RequireId(), cancellationToken);

        return new CheckoutPreview(
            location.Id, location.Name, location.AddressLine, isOpen, freeCapacity, quote.TotalAmount, quote.Currency, quote.Minutes, vehicle);
    }

    public async Task<TicketView> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var customerId = currentUser.RequireId();

        var booking = await bookings.CreateAsync(
            new NewBooking(
                customerId,
                request.LocationId,
                request.SpaceId,
                request.VehicleId,
                request.VehiclePlate.Trim().ToUpperInvariant(),
                request.VehicleType,
                request.StartsAt.UtcDateTime,
                request.EndsAt.UtcDateTime),
            cancellationToken);

        await events.PublishAsync(booking.Created(), cancellationToken);
        return await GetTicketAsync(booking.Id, cancellationToken);
    }

    public Task<IReadOnlyList<BookingSummary>> ListMineAsync(CancellationToken cancellationToken) =>
        bookings.ListForCustomerAsync(currentUser.RequireId(), cancellationToken);

    public async Task<TicketView> GetTicketAsync(Guid bookingId, CancellationToken cancellationToken) =>
        await bookings.GetTicketAsync(bookingId, currentUser.RequireId(), cancellationToken) ?? throw BookingNotFound();

    public async Task<TicketView> ExtendAsync(Guid bookingId, ExtendBookingRequest request, CancellationToken cancellationToken)
    {
        await extendValidator.ValidateAndThrowAsync(request, cancellationToken);
        var booking = await bookings.FindAsync(bookingId, cancellationToken);
        if (booking is null || booking.CustomerId != currentUser.RequireId())
        {
            throw BookingNotFound();
        }

        var extended = await bookings.ExtendAsync(bookingId, request.EndsAt.UtcDateTime, cancellationToken);
        await events.PublishAsync(extended.Extended(), cancellationToken);
        return await GetTicketAsync(bookingId, cancellationToken);
    }

    public async Task CancelAsync(Guid bookingId, CancelBookingRequest request, CancellationToken cancellationToken)
    {
        await cancelValidator.ValidateAndThrowAsync(request, cancellationToken);
        var actorId = currentUser.RequireId();
        var booking = await bookings.FindAsync(bookingId, cancellationToken) ?? throw BookingNotFound();
        if (booking.CustomerId != actorId && await bookings.FindLocationOwnerAsync(booking.LocationId, cancellationToken) != actorId)
        {
            throw BookingNotFound();
        }

        var cancelled = booking.Cancel(actorId, string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(), time.GetUtcNow().UtcDateTime);
        await bookings.SaveAsync(booking, cancellationToken);
        await events.PublishAsync(cancelled, cancellationToken);
    }

    public async Task ConfirmAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await bookings.FindAsync(bookingId, cancellationToken) ?? throw BookingNotFound();
        var confirmed = booking.Confirm(time.GetUtcNow().UtcDateTime);
        await bookings.SaveAsync(booking, cancellationToken);
        await events.PublishAsync(confirmed, cancellationToken);
    }

    internal static NotFoundException BookingNotFound() => new("booking_not_found", "Booking not found.");

    private static NotFoundException LocationNotFound() => new("location_not_found", "This parking is not available for booking.");
}
