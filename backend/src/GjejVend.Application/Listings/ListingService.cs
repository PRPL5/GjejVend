using GjejVend.Application.Common;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Listings;

namespace GjejVend.Application.Listings;

/// <summary>A parking lot as its owner sees it, including the private access instructions.</summary>
public sealed record ListingResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string AddressLine,
    string? Neighbourhood,
    string City,
    string? PostalCode,
    decimal Latitude,
    decimal Longitude,
    ParkingType ParkingType,
    InventoryMode InventoryMode,
    int TotalCapacity,
    string? ArrivalInstructions,
    string? AccessInstructions,
    string? ContactPhone,
    ListingStatus ListingStatus,
    VerificationStatus VerificationStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public interface IListingService
{
    /// <summary>Creates a draft lot for the signed-in provider.</summary>
    Task<ListingResponse> CreateAsync(ListingRequest request, CancellationToken ct = default);

    /// <summary>One of the signed-in provider's lots. Another owner's lot is Forbidden, a missing one NotFound.</summary>
    Task<ListingResponse> GetMineAsync(Guid id, CancellationToken ct = default);

    /// <summary>All lots of the signed-in provider, newest first.</summary>
    Task<IReadOnlyList<ListingResponse>> ListMineAsync(CancellationToken ct = default);

    /// <summary>Edits one of the signed-in provider's lots. Slug and status stay as they are.</summary>
    Task<ListingResponse> UpdateAsync(Guid id, ListingRequest request, CancellationToken ct = default);

    /// <summary>Soft-deletes one of the signed-in provider's lots (deleted_at is set, the row stays).</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Parking lots (M1). Ownership is checked here, not in the database: the API connects as postgres,
/// so row level security does not protect these tables for us.
/// </summary>
public sealed class ListingService(
    IListingRepository listings,
    IProviderRepository providers,
    IParkingLocationFactory factory,
    AccessGuard access,
    TimeProvider clock) : IListingService
{
    public async Task<ListingResponse> CreateAsync(ListingRequest request, CancellationToken ct = default)
    {
        var provider = await RequireProviderAsync(ct);
        var location = await factory.CreateAsync(provider.Id, request, ct);
        listings.Add(location);
        await listings.SaveChangesAsync(ct);
        return ToResponse(location);
    }

    public async Task<ListingResponse> GetMineAsync(Guid id, CancellationToken ct = default)
        => ToResponse(await FindOwnedAsync(id, ct));

    public async Task<IReadOnlyList<ListingResponse>> ListMineAsync(CancellationToken ct = default)
    {
        var provider = await RequireProviderAsync(ct);
        var lots = await listings.ListByProviderAsync(provider.Id, ct);
        return lots.Select(ToResponse).ToList();
    }

    public async Task<ListingResponse> UpdateAsync(Guid id, ListingRequest request, CancellationToken ct = default)
    {
        var location = await FindOwnedAsync(id, ct);
        factory.Apply(location, request);
        await listings.SaveChangesAsync(ct);
        return ToResponse(location);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var location = await FindOwnedAsync(id, ct);
        location.DeletedAt = clock.GetUtcNow().UtcDateTime;
        await listings.SaveChangesAsync(ct);
    }

    private async Task<ParkingProvider> RequireProviderAsync(CancellationToken ct)
    {
        var userId = access.RequireUser();
        return await providers.FindByOwnerAsync(userId, ct)
            ?? throw new ForbiddenException("Create your provider profile before listing a lot.");
    }

    private async Task<ParkingLocation> FindOwnedAsync(Guid id, CancellationToken ct)
    {
        var provider = await RequireProviderAsync(ct);
        var location = await listings.FindByIdAsync(id, ct) ?? throw new NotFoundException("Parking lot not found.");
        if (location.ProviderId != provider.Id) throw new ForbiddenException("This lot belongs to another owner.");
        return location;
    }

    private static ListingResponse ToResponse(ParkingLocation l) => new(
        l.Id, l.Name, l.Slug, l.Description, l.AddressLine, l.Neighbourhood, l.City, l.PostalCode,
        l.Latitude, l.Longitude, l.ParkingType, l.InventoryMode, l.TotalCapacity,
        l.ArrivalInstructions, l.AccessInstructions, l.ContactPhone,
        l.ListingStatus, l.VerificationStatus, l.CreatedAt, l.UpdatedAt);
}
