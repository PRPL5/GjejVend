using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Listings;

/// <summary>Data access for parking_locations. Soft-deleted lots are hidden except for the slug check.</summary>
public interface IListingRepository
{
    /// <summary>The lot with this id, or null when it does not exist or was deleted.</summary>
    Task<ParkingLocation?> FindByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>All lots of one provider, newest first.</summary>
    Task<IReadOnlyList<ParkingLocation>> ListByProviderAsync(Guid providerId, CancellationToken ct = default);

    /// <summary>True when any lot, deleted ones included, already uses the slug (the unique index covers all rows).</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);

    void Add(ParkingLocation location);

    Task SaveChangesAsync(CancellationToken ct = default);
}

public sealed class ListingRepository(GjejVendDbContext db) : IListingRepository
{
    public Task<ParkingLocation?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Set<ParkingLocation>().FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null, ct);

    public async Task<IReadOnlyList<ParkingLocation>> ListByProviderAsync(Guid providerId, CancellationToken ct = default) =>
        await db.Set<ParkingLocation>().AsNoTracking()
            .Where(l => l.ProviderId == providerId && l.DeletedAt == null)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        db.Set<ParkingLocation>().AnyAsync(l => l.Slug == slug, ct);

    public void Add(ParkingLocation location) => db.Add(location);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
