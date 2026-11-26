using GjejVend.Domain.Bookings;
using GjejVend.Domain.Identity;

namespace GjejVend.Infrastructure.Bookings;

public sealed record NewBooking(
    Guid CustomerId,
    Guid LocationId,
    Guid? SpaceId,
    Guid? VehicleId,
    string VehiclePlate,
    VehicleType VehicleType,
    DateTime StartsAt,
    DateTime EndsAt);

public sealed record TicketView(
    Guid Id,
    string Reference,
    BookingStatus Status,
    Guid LocationId,
    string LocationName,
    string AddressLine,
    decimal Latitude,
    decimal Longitude,
    string? ArrivalInstructions,
    string? AccessInstructions,
    string VehiclePlate,
    VehicleType VehicleType,
    DateTime StartsAt,
    DateTime EndsAt,
    DateTime? ExpiresAt,
    decimal TotalAmount,
    string Currency,
    string? TicketToken,
    DateTime? CheckedInAt,
    DateTime? CheckedOutAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    bool HasReview);

public sealed record BookingSummary(
    Guid Id,
    string Reference,
    BookingStatus Status,
    string LocationName,
    string VehiclePlate,
    DateTime StartsAt,
    DateTime EndsAt,
    decimal TotalAmount,
    string Currency);

public sealed record ArrivalView(
    Guid Id,
    string Reference,
    BookingStatus Status,
    Guid LocationId,
    string LocationName,
    string VehiclePlate,
    VehicleType VehicleType,
    DateTime StartsAt,
    DateTime EndsAt,
    DateTime? CheckedInAt,
    DateTime? CheckedOutAt);

public sealed record LocationSummary(Guid Id, string Name, string AddressLine, Guid OwnerId);

public sealed record DefaultVehicle(Guid Id, string Plate, VehicleType VehicleType);
