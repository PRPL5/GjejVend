namespace GjejVend.Domain.Pricing;

public sealed class OperatingHours
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public int Weekday { get; set; }
    public TimeOnly OpensAt { get; set; }
    public TimeOnly ClosesAt { get; set; }
}
