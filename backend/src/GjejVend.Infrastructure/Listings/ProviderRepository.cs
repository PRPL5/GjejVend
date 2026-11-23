using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Listings;

public interface IProviderRepository
{
    Task<ParkingProvider?> FindByOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<bool> ExistsForOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<bool> HasRoleAsync(Guid userId, Role role, CancellationToken ct = default);
    void Add(ParkingProvider provider);
    void AddRole(UserRole userRole);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public sealed class ProviderRepository(GjejVendDbContext db) : IProviderRepository
{
    public Task<ParkingProvider?> FindByOwnerAsync(Guid ownerId, CancellationToken ct = default) =>
        db.Set<ParkingProvider>().FirstOrDefaultAsync(p => p.OwnerId == ownerId && p.DeletedAt == null, ct);

    public Task<bool> ExistsForOwnerAsync(Guid ownerId, CancellationToken ct = default) =>
        db.Set<ParkingProvider>().AnyAsync(p => p.OwnerId == ownerId && p.DeletedAt == null, ct);

    public Task<bool> HasRoleAsync(Guid userId, Role role, CancellationToken ct = default) =>
        db.Set<UserRole>().AnyAsync(r => r.UserId == userId && r.Role == role, ct);

    public void Add(ParkingProvider provider) => db.Add(provider);

    public void AddRole(UserRole userRole) => db.Add(userRole);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
