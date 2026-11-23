using GjejVend.Domain.Common;

namespace GjejVend.Domain.Payments;

/// <summary>Append-only financial movement. Never updated or deleted after insert.</summary>
public sealed class LedgerEntry : Entity
{
    private LedgerEntry() { }

    public LedgerEntryType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public Guid? BookingId { get; private set; }
    public Guid? PaymentId { get; private set; }
    public Guid? RefundId { get; private set; }
    public Guid? PayoutId { get; private set; }
    public Guid? ProviderId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static LedgerEntry Create(LedgerEntryType type, decimal amount, string currency, DateTime now,
        Guid? bookingId = null, Guid? paymentId = null, Guid? refundId = null,
        Guid? payoutId = null, Guid? providerId = null)
        => new()
        {
            Type = type,
            Amount = amount,
            Currency = currency,
            CreatedAt = now,
            BookingId = bookingId,
            PaymentId = paymentId,
            RefundId = refundId,
            PayoutId = payoutId,
            ProviderId = providerId,
        };
}
