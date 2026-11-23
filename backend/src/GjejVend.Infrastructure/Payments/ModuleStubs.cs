using Microsoft.Extensions.Logging;
using GjejVend.Application.Common;

namespace GjejVend.Infrastructure.Payments;

// Temporary stand-ins for ports owned by M1 and M2. Replace the registrations in
// DependencyInjection when those modules land; M3 code only sees the interfaces.

internal sealed class UnavailableBookingReader : IBookingReader
{
    public Task<BookingPaymentInfo?> GetForPaymentAsync(Guid bookingId, CancellationToken ct = default)
        => Task.FromResult<BookingPaymentInfo?>(null);
}

internal sealed class LoggingLotModerationService(Microsoft.Extensions.Logging.ILogger<LoggingLotModerationService> log)
    : ILotModerationService
{
    public Task VerifyProviderAsync(Guid providerId, CancellationToken ct = default) => Log("verify provider", providerId);
    public Task PublishLotAsync(Guid lotId, CancellationToken ct = default) => Log("publish lot", lotId);
    public Task RejectLotAsync(Guid lotId, string reason, CancellationToken ct = default) => Log("reject lot", lotId);
    public Task SuspendLotAsync(Guid lotId, string reason, CancellationToken ct = default) => Log("suspend lot", lotId);
    public Task HideReviewAsync(Guid reviewId, string reason, CancellationToken ct = default) => Log("hide review", reviewId);

    private Task Log(string action, Guid id)
    {
        log.LogWarning("M1 moderation not wired yet: {Action} {Id}", action, id);
        return Task.CompletedTask;
    }
}
