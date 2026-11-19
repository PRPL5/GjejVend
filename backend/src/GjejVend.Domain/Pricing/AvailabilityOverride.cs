namespace GjejVend.Domain.Pricing;

public sealed class AvailabilityOverride
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public Guid? SpaceId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public OverrideKind Kind { get; set; }
    public int? Capacity { get; set; }
    public string? Reason { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
