namespace GjejVend.Application.Common;

// Ports owned by M3 and implemented by the other modules (M1 listings, M2 bookings).
// M3 depends only on these interfaces.

public sealed record BookingPaymentInfo(
    Guid BookingId,
    Guid DriverId,
    Guid ProviderId,
    decimal TotalAmount,
    string Currency,
    DateTime StartsAt,
    string Status,
    string Reference);

/// <summary>Implemented by M2 (Bookings).</summary>
public interface IBookingReader
{
    Task<BookingPaymentInfo?> GetForPaymentAsync(Guid bookingId, CancellationToken ct = default);
}

/// <summary>Implemented by M1 (Listings). M3 asks M1 to publish or suspend after review.</summary>
public interface ILotModerationService
{
    Task VerifyProviderAsync(Guid providerId, CancellationToken ct = default);
    Task PublishLotAsync(Guid lotId, CancellationToken ct = default);
    Task RejectLotAsync(Guid lotId, string reason, CancellationToken ct = default);
    Task SuspendLotAsync(Guid lotId, string reason, CancellationToken ct = default);
    Task HideReviewAsync(Guid reviewId, string reason, CancellationToken ct = default);
}
