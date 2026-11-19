namespace GjejVend.Domain.Engagement;

public sealed class FavoriteLocation
{
    public Guid UserId { get; set; }
    public Guid LocationId { get; set; }
    public DateTime CreatedAt { get; set; }
}
