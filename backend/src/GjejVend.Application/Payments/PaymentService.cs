using System.Text.Json;
using GjejVend.Application.Common;
using GjejVend.Domain.Bookings;
using GjejVend.Domain.Payments;
using GjejVend.Infrastructure.Payments;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Payments;

public sealed record PayBookingResult(Guid PaymentId, string Reference, string CheckoutUrl, decimal Amount, string Currency);

public interface IPaymentService
{
    Task<PayBookingResult> PayBookingAsync(Guid bookingId, CancellationToken ct = default);
}

public sealed class PaymentService(
    GjejVendDbContext db,
    IPaymentGateway gateway,
    AccessGuard access,
    TimeProvider clock) : IPaymentService
{
    public async Task<PayBookingResult> PayBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var userId = access.RequireUser();
        var booking = await db.Set<Booking>().AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookingId, ct)
                      ?? throw new NotFoundException("Booking not found.");

        if (booking.CustomerId != userId) throw new ForbiddenException("This booking belongs to another user.");
        if (booking.Status != BookingStatus.Pending)
            throw new BusinessRuleException($"The booking is {booking.Status}, so it can no longer be paid.");
        if (booking.ExpiresAt is { } expiresAt && expiresAt <= clock.GetUtcNow().UtcDateTime)
            throw new BusinessRuleException("The payment window for this booking has expired.");

        var payments = await db.Set<Payment>().Where(p => p.BookingId == bookingId).ToListAsync(ct);
        if (payments.Any(p => p.Status is PaymentStatus.Succeeded or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded))
            throw new BusinessRuleException("This booking is already paid.");

        // A double click must not create a second charge at the provider: reuse the open payment.
        var open = payments.FirstOrDefault(p => p.Status == PaymentStatus.Pending && p.ProviderReference is not null);
        if (open is not null)
            return new PayBookingResult(open.Id, open.ProviderReference!, ReadCheckoutUrl(open), open.Amount, open.Currency);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            CustomerId = userId,
            Amount = booking.TotalAmount,
            Currency = booking.Currency,
            Provider = gateway.Name,
            Status = PaymentStatus.Pending,
        };

        var checkout = await gateway.CreatePaymentAsync(
            new GatewayPaymentRequest(payment.Id, booking.Id, payment.Amount, payment.Currency,
                $"GjejVend booking {booking.Reference}"), ct);

        payment.ProviderReference = checkout.Reference;
        payment.Metadata = JsonSerializer.Serialize(new { checkout_url = checkout.CheckoutUrl });

        db.Set<Payment>().Add(payment);
        await db.SaveChangesAsync(ct);
        return new PayBookingResult(payment.Id, checkout.Reference, checkout.CheckoutUrl, payment.Amount, payment.Currency);
    }

    private static string ReadCheckoutUrl(Payment payment)
    {
        using var document = JsonDocument.Parse(payment.Metadata);
        return document.RootElement.TryGetProperty("checkout_url", out var url) ? url.GetString() ?? string.Empty : string.Empty;
    }
}
