using GjejVend.Domain.Identity;

namespace GjejVend.Domain.Listings;

public sealed class ParkingSpace
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public required string Label { get; set; }
    public VehicleType VehicleType { get; set; }
    public bool IsCovered { get; set; }
    public bool IsAccessible { get; set; }
    public bool HasEvCharger { get; set; }
    public int? LengthCm { get; set; }
    public int? WidthCm { get; set; }
    public int? HeightLimitCm { get; set; }
    public SpaceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
