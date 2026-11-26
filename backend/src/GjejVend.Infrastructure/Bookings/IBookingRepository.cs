using GjejVend.Domain.Bookings;

namespace GjejVend.Infrastructure.Bookings;

public interface IBookingRepository
{
    Task<Booking> CreateAsync(NewBooking booking, CancellationToken cancellationToken);

    Task<Booking> ExtendAsync(Guid bookingId, DateTime endsAt, CancellationToken cancellationToken);

    Task<Booking?> FindAsync(Guid bookingId, CancellationToken cancellationToken);

    Task<Booking?> FindByTicketTokenAsync(string ticketToken, CancellationToken cancellationToken);

    Task<IReadOnlyList<Booking>> ListOverdueAsync(DateTime now, int limit, CancellationToken cancellationToken);

    Task SaveAsync(Booking booking, CancellationToken cancellationToken);

    void Discard(Booking booking);

    Task<LocationSummary?> FindBookableLocationAsync(Guid locationId, CancellationToken cancellationToken);

    Task<Guid?> FindLocationOwnerAsync(Guid locationId, CancellationToken cancellationToken);

    Task<DefaultVehicle?> FindDefaultVehicleAsync(Guid userId, CancellationToken cancellationToken);

    Task<TicketView?> GetTicketAsync(Guid bookingId, Guid customerId, CancellationToken cancellationToken);

    Task<IReadOnlyList<BookingSummary>> ListForCustomerAsync(Guid customerId, CancellationToken cancellationToken);

    Task<ArrivalView?> GetArrivalAsync(Guid bookingId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ArrivalView>> ListArrivalsAsync(Guid ownerId, DateTime from, DateTime to, CancellationToken cancellationToken);
}
