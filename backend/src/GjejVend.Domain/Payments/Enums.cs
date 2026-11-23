namespace GjejVend.Domain.Payments;

public enum PaymentStatus { Pending, Paid, Failed, Refunded }

public enum RefundStatus { Pending, Completed, Failed }

public enum PayoutStatus { Pending, Paid, Failed }

public enum LedgerEntryType
{
    Payment,
    Refund,
    Commission,
    ProviderEarning,
    CommissionReversal,
    EarningReversal,
    Payout,
}
