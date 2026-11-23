using GjejVend.Domain.Common;

namespace GjejVend.Domain.Payments;

public sealed class Payment : Entity
{
    private Payment() { }

    public Guid BookingId { get; private set; }
    public Guid PaidByUserId { get; private set; }
    public Guid ProviderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public string PaymentProvider { get; private set; } = string.Empty;
    public string? ProviderReference { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>Amount already refunded; never exceeds <see cref="Amount"/>.</summary>
    public decimal RefundedAmount { get; private set; }

    public decimal RefundableAmount => Amount - RefundedAmount;

    public static Payment Create(Guid bookingId, Guid paidByUserId, Guid providerId, decimal amount,
        string currency, string paymentProvider, DateTime now)
    {
        if (amount <= 0) throw new DomainException("Payment amount must be positive.");
        return new Payment
        {
            BookingId = bookingId,
            PaidByUserId = paidByUserId,
            ProviderId = providerId,
            Amount = amount,
            Currency = currency,
            PaymentProvider = paymentProvider,
            Status = PaymentStatus.Pending,
            CreatedAt = now,
        };
    }

    public void AttachProviderReference(string reference) => ProviderReference = reference;

    public void MarkPaid(DateTime now)
    {
        if (Status == PaymentStatus.Paid) return; // idempotent
        if (Status != PaymentStatus.Pending)
            throw new DomainException($"Cannot mark a {Status} payment as paid.");
        Status = PaymentStatus.Paid;
        PaidAt = now;
    }

    public void MarkFailed()
    {
        if (Status == PaymentStatus.Failed) return;
        if (Status != PaymentStatus.Pending)
            throw new DomainException($"Cannot fail a {Status} payment.");
        Status = PaymentStatus.Failed;
    }

    public void ApplyRefund(decimal amount)
    {
        if (Status != PaymentStatus.Paid)
            throw new DomainException("Only paid payments can be refunded.");
        if (amount <= 0) throw new DomainException("Refund amount must be positive.");
        if (amount > RefundableAmount) throw new DomainException("Refund exceeds the payment amount.");
        RefundedAmount += amount;
        if (RefundedAmount == Amount) Status = PaymentStatus.Refunded;
    }
}
