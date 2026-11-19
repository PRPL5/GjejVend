namespace GjejVend.Domain.Listings;

public sealed class ParkingLocationAmenity
{
    public Guid LocationId { get; set; }
    public required string AmenityCode { get; set; }
}
