namespace GjejVend.Domain.Payments;

public sealed class ProviderPayout
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public PayoutStatus Status { get; set; }
    public string? ProviderReference { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
