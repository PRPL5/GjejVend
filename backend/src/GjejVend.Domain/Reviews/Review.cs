namespace GjejVend.Domain.Reviews;

public sealed class Review
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid LocationId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public ReviewStatus Status { get; set; }
    public string? ProviderResponse { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
