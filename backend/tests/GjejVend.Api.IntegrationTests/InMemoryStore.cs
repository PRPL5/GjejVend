using System.Collections.Concurrent;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Identity;
using GjejVend.Infrastructure.Listings;

namespace GjejVend.Api.IntegrationTests;

/// <summary>In-memory stand-in for the database, so API tests run without Postgres.</summary>
public sealed class InMemoryStore : IUserRoleRepository, IProviderRepository, IListingRepository
{
    private readonly ConcurrentDictionary<Guid, ParkingProvider> _providers = new();
    private readonly ConcurrentDictionary<Guid, ParkingLocation> _locations = new();
    private readonly ConcurrentDictionary<(Guid UserId, Role Role), UserRole> _roles = new();
    private readonly List<object> _pending = [];

    public Task<IReadOnlyList<Role>> GetRolesAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Role>>(_roles.Keys.Where(k => k.UserId == userId).Select(k => k.Role).ToList());

    public Task<ParkingProvider?> FindByOwnerAsync(Guid ownerId, CancellationToken ct = default) =>
        Task.FromResult(_providers.TryGetValue(ownerId, out var p) ? p : null);

    public Task<bool> ExistsForOwnerAsync(Guid ownerId, CancellationToken ct = default) =>
        Task.FromResult(_providers.ContainsKey(ownerId));

    public Task<bool> HasRoleAsync(Guid userId, Role role, CancellationToken ct = default) =>
        Task.FromResult(_roles.ContainsKey((userId, role)));

    public void Add(ParkingProvider provider)
    {
        lock (_pending) _pending.Add(provider);
    }

    public void AddRole(UserRole userRole)
    {
        lock (_pending) _pending.Add(userRole);
    }

    public Task<ParkingLocation?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_locations.TryGetValue(id, out var l) && l.DeletedAt == null ? l : null);

    public Task<IReadOnlyList<ParkingLocation>> ListByProviderAsync(Guid providerId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ParkingLocation>>(_locations.Values
            .Where(l => l.ProviderId == providerId && l.DeletedAt == null)
            .OrderByDescending(l => l.CreatedAt)
            .ToList());

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        Task.FromResult(_locations.Values.Any(l => l.Slug == slug));

    public void Add(ParkingLocation location)
    {
        lock (_pending) _pending.Add(location);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        lock (_pending)
        {
            foreach (var item in _pending)
            {
                switch (item)
                {
                    case ParkingProvider p:
                        if (p.Id == Guid.Empty) p.Id = Guid.NewGuid();
                        if (p.CreatedAt == default) p.CreatedAt = DateTime.UtcNow;
                        _providers[p.OwnerId] = p;
                        break;
                    case UserRole r:
                        _roles[(r.UserId, r.Role)] = r;
                        break;
                    case ParkingLocation l:
                        if (l.Id == Guid.Empty) l.Id = Guid.NewGuid();
                        if (l.CreatedAt == default) l.CreatedAt = l.UpdatedAt = DateTime.UtcNow;
                        _locations[l.Id] = l;
                        break;
                }
            }
            _pending.Clear();
        }
        return Task.CompletedTask;
    }
}
