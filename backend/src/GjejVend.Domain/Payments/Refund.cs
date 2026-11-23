using GjejVend.Domain.Common;

namespace GjejVend.Domain.Payments;

public sealed class Refund : Entity
{
    private Refund() { }

    public Guid PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public RefundStatus Status { get; private set; }
    public string? ProviderReference { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Refund Create(Guid paymentId, decimal amount, string reason, DateTime now)
    {
        if (amount <= 0) throw new DomainException("Refund amount must be positive.");
        return new Refund
        {
            PaymentId = paymentId,
            Amount = amount,
            Reason = reason,
            Status = RefundStatus.Pending,
            CreatedAt = now,
        };
    }

    public void Complete(string providerReference)
    {
        ProviderReference = providerReference;
        Status = RefundStatus.Completed;
    }

    public void Fail() => Status = RefundStatus.Failed;
}
