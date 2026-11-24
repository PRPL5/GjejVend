using GjejVend.Application.Common;
using GjejVend.Domain.Identity;
using GjejVend.Infrastructure.Identity;

namespace GjejVend.Application.Identity;

public interface IRoleService
{
    /// <summary>Roles of the signed-in user, used by GET /me.</summary>
    Task<IReadOnlyList<Role>> GetMyRolesAsync(CancellationToken ct = default);
}

public sealed class RoleService(IUserRoleRepository roles, AccessGuard access) : IRoleService
{
    public Task<IReadOnlyList<Role>> GetMyRolesAsync(CancellationToken ct = default) =>
        roles.GetRolesAsync(access.RequireUser(), ct);
}
