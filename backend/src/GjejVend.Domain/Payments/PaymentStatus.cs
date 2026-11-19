namespace GjejVend.Domain.Payments;

public enum PaymentStatus
{
    Pending,
    Authorized,
    Succeeded,
    Failed,
    PartiallyRefunded,
    Refunded,
}
