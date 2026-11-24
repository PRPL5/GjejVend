using GjejVend.Application.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers.Listings;

[ApiController]
[Authorize]
[Route("api/providers")]
public sealed class ProvidersController(IProviderService providers) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProviderResponse>> Create(ProviderRequest request, CancellationToken ct)
    {
        var provider = await providers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetMine), provider);
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProviderResponse>> GetMine(CancellationToken ct) =>
        Ok(await providers.GetMineAsync(ct));

    [HttpPut("me")]
    public async Task<ActionResult<ProviderResponse>> UpdateMine(ProviderRequest request, CancellationToken ct) =>
        Ok(await providers.UpdateMineAsync(request, ct));
}
