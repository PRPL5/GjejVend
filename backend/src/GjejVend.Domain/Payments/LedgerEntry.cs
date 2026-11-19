namespace GjejVend.Domain.Payments;

public sealed class LedgerEntry
{
    public Guid Id { get; set; }
    public LedgerEntryType EntryType { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? RefundId { get; set; }
    public Guid? PayoutId { get; set; }
    public Guid? ProviderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime CreatedAt { get; set; }
}
