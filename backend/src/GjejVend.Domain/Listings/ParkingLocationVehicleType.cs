using GjejVend.Domain.Identity;

namespace GjejVend.Domain.Listings;

public sealed class ParkingLocationVehicleType
{
    public Guid LocationId { get; set; }
    public VehicleType VehicleType { get; set; }
}
