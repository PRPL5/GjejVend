using GjejVend.Domain.Identity;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Identity;

/// <summary>Reads the roles a user holds in user_roles (customer, provider, admin).</summary>
public interface IUserRoleRepository
{
    Task<IReadOnlyList<Role>> GetRolesAsync(Guid userId, CancellationToken ct = default);
}

public sealed class UserRoleRepository(GjejVendDbContext db) : IUserRoleRepository
{
    public async Task<IReadOnlyList<Role>> GetRolesAsync(Guid userId, CancellationToken ct = default) =>
        await db.Set<UserRole>()
            .Where(r => r.UserId == userId)
            .Select(r => r.Role)
            .ToListAsync(ct);
}
