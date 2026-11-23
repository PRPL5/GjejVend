using GjejVend.Domain.Identity;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Common;

/// <summary>Authorization checks shared by the M3 services. Roles live in the user_roles table.</summary>
public sealed class AccessGuard(ICurrentUser currentUser, GjejVendDbContext db)
{
    public Guid RequireUser()
        => currentUser.Id ?? throw new ForbiddenException("Sign in to continue.");

    public async Task<Guid> RequireRoleAsync(Role role, CancellationToken ct = default)
    {
        var userId = RequireUser();
        var hasRole = await db.Set<UserRole>().AnyAsync(r => r.UserId == userId && r.Role == role, ct);
        return hasRole ? userId : throw new ForbiddenException($"This action requires the {role} role.");
    }

    public Task<Guid> RequireAdminAsync(CancellationToken ct = default) => RequireRoleAsync(Role.Admin, ct);
}
