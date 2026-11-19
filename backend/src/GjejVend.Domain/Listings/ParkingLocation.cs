namespace GjejVend.Domain.Listings;

public sealed class ParkingLocation
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public required string AddressLine { get; set; }
    public string? Neighbourhood { get; set; }
    public required string City { get; set; }
    public string? PostalCode { get; set; }
    public string CountryCode { get; set; } = "XK";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Timezone { get; set; } = "Europe/Belgrade";
    public ParkingType ParkingType { get; set; }
    public InventoryMode InventoryMode { get; set; }
    public int TotalCapacity { get; set; }
    public int OfflineOccupied { get; set; }
    public ListingStatus ListingStatus { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public string? ArrivalInstructions { get; set; }
    public string? AccessInstructions { get; set; }
    public string? ContactPhone { get; set; }
    public decimal? RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
