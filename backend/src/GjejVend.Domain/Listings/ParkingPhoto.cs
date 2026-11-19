namespace GjejVend.Domain.Listings;

public sealed class ParkingPhoto
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public required string StoragePath { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
}
