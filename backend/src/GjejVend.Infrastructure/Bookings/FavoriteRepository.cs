using GjejVend.Domain.Engagement;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Bookings;

public interface IFavoriteRepository
{
    Task<IReadOnlyList<FavoriteView>> ListAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> LocationExistsAsync(Guid locationId, CancellationToken cancellationToken);

    Task AddAsync(Guid userId, Guid locationId, CancellationToken cancellationToken);

    Task RemoveAsync(Guid userId, Guid locationId, CancellationToken cancellationToken);
}

public sealed record FavoriteView(
    Guid LocationId,
    string Name,
    string Slug,
    string AddressLine,
    string? Neighbourhood,
    decimal? RatingAverage,
    DateTime SavedAt);

internal sealed class FavoriteRepository(GjejVendDbContext context) : IFavoriteRepository
{
    public async Task<IReadOnlyList<FavoriteView>> ListAsync(Guid userId, CancellationToken cancellationToken) =>
        await context.Set<FavoriteLocation>()
            .Where(f => f.UserId == userId)
            .Join(context.Set<ParkingLocation>().Where(l => l.DeletedAt == null), f => f.LocationId, l => l.Id, (f, l) => new { Favorite = f, Location = l })
            .OrderByDescending(x => x.Favorite.CreatedAt)
            .Select(x => new FavoriteView(
                x.Location.Id,
                x.Location.Name,
                x.Location.Slug,
                x.Location.AddressLine,
                x.Location.Neighbourhood,
                x.Location.RatingAverage,
                x.Favorite.CreatedAt))
            .ToListAsync(cancellationToken);

    public Task<bool> LocationExistsAsync(Guid locationId, CancellationToken cancellationToken) =>
        context.Set<ParkingLocation>().AnyAsync(l => l.Id == locationId && l.DeletedAt == null, cancellationToken);

    public Task AddAsync(Guid userId, Guid locationId, CancellationToken cancellationToken) =>
        context.Database.ExecuteSqlAsync(
            $"INSERT INTO public.favorite_locations (user_id, location_id) VALUES ({userId}, {locationId}) ON CONFLICT DO NOTHING",
            cancellationToken);

    public Task RemoveAsync(Guid userId, Guid locationId, CancellationToken cancellationToken) =>
        context.Set<FavoriteLocation>()
            .Where(f => f.UserId == userId && f.LocationId == locationId)
            .ExecuteDeleteAsync(cancellationToken);
}
