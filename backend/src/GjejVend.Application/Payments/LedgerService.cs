using GjejVend.Application.Common;
using GjejVend.Domain.Payments;

namespace GjejVend.Application.Payments;

public interface ILedgerService
{
    Task RecordPaymentAsync(Payment payment, CommissionBreakdown split, CancellationToken ct = default);
}

/// <summary>Writes append-only ledger rows. Rows are never updated or deleted.</summary>
public sealed class LedgerService(IApplicationDbContext db, IClock clock) : ILedgerService
{
    public Task RecordPaymentAsync(Payment payment, CommissionBreakdown split, CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        db.LedgerEntries.Add(LedgerEntry.Create(LedgerEntryType.Payment, payment.Amount, payment.Currency, now,
            payment.BookingId, payment.Id, providerId: payment.ProviderId));
        db.LedgerEntries.Add(LedgerEntry.Create(LedgerEntryType.Commission, split.Commission, payment.Currency, now,
            payment.BookingId, payment.Id, providerId: payment.ProviderId));
        db.LedgerEntries.Add(LedgerEntry.Create(LedgerEntryType.ProviderEarning, split.ProviderAmount, payment.Currency, now,
            payment.BookingId, payment.Id, providerId: payment.ProviderId));
        return Task.CompletedTask;
    }
}
