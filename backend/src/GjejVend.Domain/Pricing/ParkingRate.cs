namespace GjejVend.Domain.Pricing;

public sealed class ParkingRate
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public Guid? SpaceId { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public decimal? MonthlyRate { get; set; }
    public string Currency { get; set; } = "EUR";
    public int MinMinutes { get; set; } = 60;
    public int? MaxMinutes { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
