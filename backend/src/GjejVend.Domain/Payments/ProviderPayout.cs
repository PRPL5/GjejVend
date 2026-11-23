using GjejVend.Domain.Common;

namespace GjejVend.Domain.Payments;

public sealed class ProviderPayout : Entity
{
    private ProviderPayout() { }

    public Guid ProviderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public DateTime PeriodFrom { get; private set; }
    public DateTime PeriodTo { get; private set; }
    public PayoutStatus Status { get; private set; }
    public string? ProviderReference { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public static ProviderPayout Create(Guid providerId, decimal amount, string currency,
        DateTime periodFrom, DateTime periodTo)
    {
        if (amount <= 0) throw new DomainException("Payout amount must be positive.");
        if (periodTo <= periodFrom) throw new DomainException("Payout period is invalid.");
        return new ProviderPayout
        {
            ProviderId = providerId,
            Amount = amount,
            Currency = currency,
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            Status = PayoutStatus.Pending,
        };
    }

    public void MarkPaid(string providerReference, DateTime now)
    {
        if (Status == PayoutStatus.Paid) return;
        ProviderReference = providerReference;
        Status = PayoutStatus.Paid;
        PaidAt = now;
    }

    public void MarkFailed() => Status = PayoutStatus.Failed;
}
