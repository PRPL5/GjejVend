using GjejVend.Application.Identity;
using GjejVend.Domain.Identity;
using GjejVend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers;

[ApiController]
[Authorize]
[Route("me")]
public sealed class MeController(ICurrentUser currentUser, IRoleService roles) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MeResponse>> Get(CancellationToken ct)
    {
        if (currentUser.Id is not { } id) return Unauthorized();
        return new MeResponse(id, await roles.GetMyRolesAsync(ct));
    }
}

public sealed record MeResponse(Guid Id, IReadOnlyList<Role> Roles);
