namespace GjejVend.Domain.Identity;

public sealed class Vehicle
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Plate { get; set; }
    public VehicleType VehicleType { get; set; }
    public string? Label { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
