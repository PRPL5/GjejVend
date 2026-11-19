using GjejVend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers;

[ApiController]
[Authorize]
[Route("me")]
public sealed class MeController(ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public ActionResult<MeResponse> Get() =>
        currentUser.Id is { } id ? new MeResponse(id) : Unauthorized();
}

public sealed record MeResponse(Guid Id);
