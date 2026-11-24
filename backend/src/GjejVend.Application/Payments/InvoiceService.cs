using GjejVend.Application.Common;
using GjejVend.Domain.Payments;
using GjejVend.Infrastructure.Payments;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Payments;

public sealed record PaidBookingSummary(
    Guid BookingId, Guid PaymentId, decimal Amount, string Currency, DateTime? PaidAt, string Status);

public sealed record BookingInvoice(
    Guid BookingId, Guid PaymentId, decimal Amount, string Currency, DateTime? PaidAt,
    string? Number, string? HostedUrl, string? PdfUrl);

public interface IInvoiceService
{
    /// <summary>The caller's paid bookings, newest first. No provider call, so it is cheap to list.</summary>
    Task<IReadOnlyList<PaidBookingSummary>> ListMineAsync(CancellationToken ct = default);

    /// <summary>The Stripe-hosted invoice and PDF for one of the caller's paid bookings.</summary>
    Task<BookingInvoice> GetForBookingAsync(Guid bookingId, CancellationToken ct = default);
}

/// <summary>Invoices are created and hosted by Stripe; this service only looks them up.</summary>
public sealed class InvoiceService(GjejVendDbContext db, IPaymentGateway gateway, AccessGuard access) : IInvoiceService
{
    private static readonly PaymentStatus[] PaidStatuses =
        [PaymentStatus.Succeeded, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];

    public async Task<IReadOnlyList<PaidBookingSummary>> ListMineAsync(CancellationToken ct = default)
    {
        var userId = access.RequireUser();
        var rows = await db.Set<Payment>().AsNoTracking()
            .Where(p => p.CustomerId == userId && PaidStatuses.Contains(p.Status))
            .OrderByDescending(p => p.PaidAt)
            .Select(p => new { p.BookingId, p.Id, p.Amount, p.Currency, p.PaidAt, p.Status })
            .ToListAsync(ct);
        return rows.Select(p => new PaidBookingSummary(p.BookingId, p.Id, p.Amount, p.Currency, p.PaidAt, p.Status.ToString())).ToList();
    }

    public async Task<BookingInvoice> GetForBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var userId = access.RequireUser();
        var payment = await db.Set<Payment>().AsNoTracking()
            .Where(p => p.BookingId == bookingId && PaidStatuses.Contains(p.Status))
            .OrderByDescending(p => p.PaidAt)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("No paid payment was found for this booking.");

        if (payment.CustomerId != userId) throw new ForbiddenException("This invoice belongs to another user.");
        if (payment.ProviderReference is null) throw new BusinessRuleException("The payment has no provider reference.");

        var invoice = await gateway.GetInvoiceAsync(payment.ProviderReference, ct)
                      ?? throw new BusinessRuleException("The invoice is not ready yet. Try again in a moment.");

        return new BookingInvoice(bookingId, payment.Id, payment.Amount, payment.Currency, payment.PaidAt,
            invoice.Number, invoice.HostedUrl, invoice.PdfUrl);
    }
}
