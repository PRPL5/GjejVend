using GjejVend.Domain.Bookings;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Reviews;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Bookings;

internal sealed class BookingRepository(GjejVendDbContext context) : IBookingRepository
{
    private DbSet<Booking> Bookings => context.Set<Booking>();

    private IQueryable<ParkingLocation> Locations => context.Set<ParkingLocation>();

    private IQueryable<ParkingProvider> Providers => context.Set<ParkingProvider>();

    public Task<Booking> CreateAsync(NewBooking booking, CancellationToken cancellationToken)
    {
        var vehicleType = SnakeCase.From(booking.VehicleType.ToString());
        return DatabaseErrors.GuardAsync(async () => (await Bookings
            .FromSql($"""
                SELECT * FROM public.create_booking(
                    {booking.CustomerId}, {booking.LocationId}, {booking.StartsAt}, {booking.EndsAt},
                    {booking.VehiclePlate}, {vehicleType}, {booking.SpaceId}::uuid, {booking.VehicleId}::uuid)
                """)
            .AsNoTracking()
            .ToListAsync(cancellationToken)).Single());
    }

    public Task<Booking> ExtendAsync(Guid bookingId, DateTime endsAt, CancellationToken cancellationToken) =>
        DatabaseErrors.GuardAsync(async () => (await Bookings
            .FromSql($"SELECT * FROM public.extend_booking({bookingId}, {endsAt})")
            .AsNoTracking()
            .ToListAsync(cancellationToken)).Single());

    public Task<Booking?> FindAsync(Guid bookingId, CancellationToken cancellationToken) =>
        Bookings.SingleOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

    public Task<Booking?> FindByTicketTokenAsync(string ticketToken, CancellationToken cancellationToken) =>
        Bookings.SingleOrDefaultAsync(b => b.TicketToken == ticketToken, cancellationToken);

    public async Task<IReadOnlyList<Booking>> ListOverdueAsync(DateTime now, int limit, CancellationToken cancellationToken) =>
        await Bookings
            .Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active) && b.EndsAt <= now)
            .OrderBy(b => b.EndsAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public Task SaveAsync(Booking booking, CancellationToken cancellationToken) =>
        DatabaseErrors.GuardAsync(() => context.SaveChangesAsync(cancellationToken));

    public void Discard(Booking booking) => context.Entry(booking).State = EntityState.Detached;

    public Task<LocationSummary?> FindBookableLocationAsync(Guid locationId, CancellationToken cancellationToken) =>
        Locations
            .Where(l => l.Id == locationId && l.DeletedAt == null && l.ListingStatus == ListingStatus.Published)
            .Join(Providers, l => l.ProviderId, p => p.Id, (l, p) => new LocationSummary(l.Id, l.Name, l.AddressLine, p.OwnerId))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Guid?> FindLocationOwnerAsync(Guid locationId, CancellationToken cancellationToken) =>
        Locations
            .Where(l => l.Id == locationId)
            .Join(Providers, l => l.ProviderId, p => p.Id, (l, p) => (Guid?)p.OwnerId)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<DefaultVehicle?> FindDefaultVehicleAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Set<Vehicle>()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.IsDefault)
            .ThenByDescending(v => v.UpdatedAt)
            .Select(v => new DefaultVehicle(v.Id, v.Plate, v.VehicleType))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<TicketView?> GetTicketAsync(Guid bookingId, Guid customerId, CancellationToken cancellationToken) =>
        Bookings
            .Where(b => b.Id == bookingId && b.CustomerId == customerId)
            .Join(Locations, b => b.LocationId, l => l.Id, (b, l) => new { Booking = b, Location = l })
            .Select(x => new TicketView(
                x.Booking.Id,
                x.Booking.Reference,
                x.Booking.Status,
                x.Location.Id,
                x.Location.Name,
                x.Location.AddressLine,
                x.Location.Latitude,
                x.Location.Longitude,
                x.Location.ArrivalInstructions,
                x.Booking.Status == BookingStatus.Confirmed || x.Booking.Status == BookingStatus.Active ? x.Location.AccessInstructions : null,
                x.Booking.VehiclePlate,
                x.Booking.VehicleType,
                x.Booking.StartsAt,
                x.Booking.EndsAt,
                x.Booking.ExpiresAt,
                x.Booking.TotalAmount,
                x.Booking.Currency,
                x.Booking.Status == BookingStatus.Confirmed || x.Booking.Status == BookingStatus.Active ? x.Booking.TicketToken : null,
                x.Booking.CheckedInAt,
                x.Booking.CheckedOutAt,
                x.Booking.CancelledAt,
                x.Booking.CancellationReason,
                context.Set<Review>().Any(r => r.BookingId == x.Booking.Id)))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<BookingSummary>> ListForCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        await Bookings
            .Where(b => b.CustomerId == customerId)
            .Join(Locations, b => b.LocationId, l => l.Id, (b, l) => new { Booking = b, Location = l })
            .OrderByDescending(x => x.Booking.StartsAt)
            .Take(200)
            .Select(x => new BookingSummary(
                x.Booking.Id,
                x.Booking.Reference,
                x.Booking.Status,
                x.Location.Name,
                x.Booking.VehiclePlate,
                x.Booking.StartsAt,
                x.Booking.EndsAt,
                x.Booking.TotalAmount,
                x.Booking.Currency))
            .ToListAsync(cancellationToken);

    public Task<ArrivalView?> GetArrivalAsync(Guid bookingId, CancellationToken cancellationToken) =>
        Arrivals(Bookings.Where(b => b.Id == bookingId)).SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ArrivalView>> ListArrivalsAsync(Guid ownerId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var ownedLocationIds = Locations
            .Join(Providers.Where(p => p.OwnerId == ownerId), l => l.ProviderId, p => p.Id, (l, _) => l.Id);

        return await Arrivals(Bookings.Where(b =>
                ownedLocationIds.Contains(b.LocationId)
                && b.Status != BookingStatus.Cancelled
                && b.Status != BookingStatus.Expired
                && b.StartsAt < to
                && b.EndsAt > from))
            .ToListAsync(cancellationToken);
    }

    private IQueryable<ArrivalView> Arrivals(IQueryable<Booking> bookings) =>
        bookings
            .Join(Locations, b => b.LocationId, l => l.Id, (b, l) => new { Booking = b, Location = l })
            .OrderBy(x => x.Booking.StartsAt)
            .Select(x => new ArrivalView(
                x.Booking.Id,
                x.Booking.Reference,
                x.Booking.Status,
                x.Location.Id,
                x.Location.Name,
                x.Booking.VehiclePlate,
                x.Booking.VehicleType,
                x.Booking.StartsAt,
                x.Booking.EndsAt,
                x.Booking.CheckedInAt,
                x.Booking.CheckedOutAt));
}
