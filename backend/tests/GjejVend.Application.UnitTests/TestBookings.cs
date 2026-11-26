using GjejVend.Domain.Bookings;

namespace GjejVend.Application.UnitTests;

internal static class TestBookings
{
    public static Booking With(BookingStatus status, Guid? locationId = null) => new()
    {
        Id = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        LocationId = locationId ?? Guid.NewGuid(),
        Reference = "GVTEST01",
        VehiclePlate = "01ABC123",
        Status = status,
    };

    public static BookingCancelled Cancelled(Booking booking) =>
        new(booking.Id, booking.CustomerId, booking.LocationId, booking.Reference, booking.CustomerId, null);
}
