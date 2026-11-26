using GjejVend.Application.Common;
using GjejVend.Domain.Engagement;
using GjejVend.Domain.Listings;
using GjejVend.Domain.Moderation;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ListingVerification = GjejVend.Domain.Listings.VerificationStatus;

namespace GjejVend.Application.Admin;

public sealed record VerificationItem(
    Guid Id, string TargetType, Guid TargetId, string TargetName, Guid SubmittedBy,
    IReadOnlyList<string> DocumentPaths, string Status, string? ReviewNotes, DateTime CreatedAt, DateTime? ReviewedAt);

public interface IVerificationService
{
    /// <summary>Admin: the verification queue, oldest first so nothing waits forever.</summary>
    Task<IReadOnlyList<VerificationItem>> ListAsync(VerificationRequestStatus? status, CancellationToken ct = default);

    /// <summary>Admin: approves or rejects a provider or lot. Approving a lot publishes it.</summary>
    Task<VerificationItem> ReviewAsync(Guid requestId, bool approve, string? notes, CancellationToken ct = default);
}

/// <summary>
/// Reviews verification requests. Status changes are recorded in audit_logs by database triggers, with the
/// acting admin taken from the connection, so no manual audit write is needed here.
/// </summary>
public sealed class VerificationService(GjejVendDbContext db, AccessGuard access, TimeProvider clock) : IVerificationService
{
    public async Task<IReadOnlyList<VerificationItem>> ListAsync(VerificationRequestStatus? status, CancellationToken ct = default)
    {
        await access.RequireAdminAsync(ct);
        var requests = await db.Set<VerificationRequest>().AsNoTracking()
            .Where(v => status == null || v.Status == status)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync(ct);
        return await ToItemsAsync(requests, ct);
    }

    public async Task<VerificationItem> ReviewAsync(Guid requestId, bool approve, string? notes, CancellationToken ct = default)
    {
        var adminId = await access.RequireAdminAsync(ct);
        var request = await db.Set<VerificationRequest>().FirstOrDefaultAsync(v => v.Id == requestId, ct)
                      ?? throw new NotFoundException("Verification request not found.");

        if (request.Status != VerificationRequestStatus.Pending)
            throw new BusinessRuleException("This request was already reviewed.");
        if (!approve && string.IsNullOrWhiteSpace(notes))
            throw new BusinessRuleException("Tell the owner why the request was rejected.");

        request.Status = approve ? VerificationRequestStatus.Approved : VerificationRequestStatus.Rejected;
        request.ReviewerId = adminId;
        request.ReviewNotes = notes?.Trim();
        request.ReviewedAt = clock.GetUtcNow().UtcDateTime;

        var targetName = await ApplyDecisionAsync(request, approve, ct);

        db.Set<Notification>().Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.SubmittedBy,
            Type = approve ? "verification_approved" : "verification_rejected",
            Title = approve ? $"{targetName} was approved" : $"{targetName} was not approved",
            Body = approve ? "Your verification request was approved." : request.ReviewNotes!,
            EntityType = "verification_request",
            EntityId = request.Id,
        });

        await db.SaveChangesAsync(ct);
        return (await ToItemsAsync([request], ct))[0];
    }

    private async Task<string> ApplyDecisionAsync(VerificationRequest request, bool approve, CancellationToken ct)
    {
        var decision = approve ? ListingVerification.Verified : ListingVerification.Rejected;

        if (request.ProviderId is { } providerId)
        {
            var provider = await db.Set<ParkingProvider>().FirstOrDefaultAsync(p => p.Id == providerId, ct)
                           ?? throw new NotFoundException("Provider not found.");
            provider.VerificationStatus = decision;
            return provider.DisplayName;
        }

        var locationId = request.LocationId ?? throw new BusinessRuleException("The request has no target.");
        var location = await db.Set<ParkingLocation>().FirstOrDefaultAsync(l => l.Id == locationId, ct)
                       ?? throw new NotFoundException("Parking lot not found.");
        location.VerificationStatus = decision;
        if (approve) location.ListingStatus = ListingStatus.Published;
        else if (location.ListingStatus == ListingStatus.PendingReview) location.ListingStatus = ListingStatus.Draft;
        return location.Name;
    }

    private async Task<IReadOnlyList<VerificationItem>> ToItemsAsync(List<VerificationRequest> requests, CancellationToken ct)
    {
        var providerIds = requests.Where(r => r.ProviderId != null).Select(r => r.ProviderId!.Value).ToList();
        var locationIds = requests.Where(r => r.LocationId != null).Select(r => r.LocationId!.Value).ToList();

        var providers = await db.Set<ParkingProvider>().AsNoTracking().Where(p => providerIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.DisplayName, ct);
        var locations = await db.Set<ParkingLocation>().AsNoTracking().Where(l => locationIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, ct);

        return requests.Select(r => r.ProviderId is { } providerId
                ? ToItem(r, "provider", providerId, providers.GetValueOrDefault(providerId, "Unknown provider"))
                : ToItem(r, "lot", r.LocationId!.Value, locations.GetValueOrDefault(r.LocationId!.Value, "Unknown lot")))
            .ToList();
    }

    private static VerificationItem ToItem(VerificationRequest r, string type, Guid targetId, string name)
        => new(r.Id, type, targetId, name, r.SubmittedBy, r.DocumentPaths.ToList(), r.Status.ToString(),
            r.ReviewNotes, r.CreatedAt, r.ReviewedAt);
}
