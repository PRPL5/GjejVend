using GjejVend.Application.Common;
using GjejVend.Application.Common.Events;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Payments;

public sealed record PayBookingResult(Guid PaymentId, string Reference, string CheckoutUrl, decimal Amount, string Currency);

public interface IPaymentService
{
    Task<PayBookingResult> PayBookingAsync(Guid bookingId, CancellationToken ct = default);
    Task<Payment?> GetPaymentForBookingAsync(Guid bookingId, CancellationToken ct = default);
}

public sealed class PaymentService(
    IApplicationDbContext db,
    IBookingReader bookings,
    IPaymentGateway gateway,
    ICurrentUser user,
    IClock clock) : IPaymentService
{
    public async Task<PayBookingResult> PayBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var userId = user.UserId ?? throw new ForbiddenException("Sign in to pay for a booking.");
        var booking = await bookings.GetForPaymentAsync(bookingId, ct)
                      ?? throw new NotFoundException("Booking not found.");

        if (booking.DriverId != userId) throw new ForbiddenException("This booking belongs to another user.");
        if (!string.Equals(booking.Status, "pending", StringComparison.OrdinalIgnoreCase))
            throw new ValidationFailedException($"Booking is {booking.Status}, it can no longer be paid.");

        var existing = await db.Payments.Where(p => p.BookingId == bookingId).ToListAsync(ct);
        if (existing.Any(p => p.Status == PaymentStatus.Paid))
            throw new ValidationFailedException("Booking is already paid.");

        // Reuse a pending payment so double clicks do not create duplicates at the provider.
        var payment = existing.FirstOrDefault(p => p.Status == PaymentStatus.Pending && p.ProviderReference is not null);
        if (payment is not null)
        {
            return new PayBookingResult(payment.Id, payment.ProviderReference!, $"{gateway.Name}://pay/{payment.ProviderReference}",
                payment.Amount, payment.Currency);
        }

        payment = Payment.Create(bookingId, userId, booking.ProviderId, booking.TotalAmount, booking.Currency,
            gateway.Name, clock.UtcNow);
        var checkout = await gateway.CreatePaymentAsync(
            new GatewayPaymentRequest(payment.Id, bookingId, payment.Amount, payment.Currency,
                $"GjejVend booking {booking.Reference}"), ct);
        payment.AttachProviderReference(checkout.Reference);

        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
        return new PayBookingResult(payment.Id, checkout.Reference, checkout.CheckoutUrl, payment.Amount, payment.Currency);
    }

    public Task<Payment?> GetPaymentForBookingAsync(Guid bookingId, CancellationToken ct = default)
        => db.Payments.AsNoTracking().Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.CreatedAt).FirstOrDefaultAsync(ct);
}

public interface IPaymentWebhookService
{
    /// <summary>Returns false when the signature is invalid. Safe to call repeatedly with the same payload.</summary>
    Task<bool> HandleAsync(string body, string? signature, CancellationToken ct = default);
}

public sealed class PaymentWebhookService(
    IApplicationDbContext db,
    IPaymentGateway gateway,
    ICommissionCalculator commission,
    ILedgerService ledger,
    IEventPublisher events,
    IClock clock) : IPaymentWebhookService
{
    public async Task<bool> HandleAsync(string body, string? signature, CancellationToken ct = default)
    {
        var evt = gateway.ParseWebhook(body, signature);
        if (evt is null) return false;

        var payment = await db.Payments.FirstOrDefaultAsync(p => p.ProviderReference == evt.Reference, ct);
        if (payment is null) return true; // unknown reference: acknowledge so the provider stops retrying

        switch (evt.Type)
        {
            case GatewayEventType.PaymentSucceeded:
                if (payment.Status == PaymentStatus.Paid) return true; // duplicate delivery
                payment.MarkPaid(clock.UtcNow);
                var split = await commission.CalculateAsync(payment.Amount, ct);
                await ledger.RecordPaymentAsync(payment, split, ct);
                await db.SaveChangesAsync(ct);
                await events.PublishAsync(new PaymentCompleted(payment.BookingId, payment.Id, payment.Amount, payment.Currency), ct);
                break;

            case GatewayEventType.PaymentFailed:
                if (payment.Status != PaymentStatus.Pending) return true;
                payment.MarkFailed();
                await db.SaveChangesAsync(ct);
                await events.PublishAsync(new PaymentFailed(payment.BookingId, payment.Id, evt.FailureReason ?? "declined"), ct);
                break;
        }

        return true;
    }
}
