using GjejVend.Application.Bookings;
using GjejVend.Infrastructure.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers.Bookings;

[ApiController]
[Authorize]
[Route("me/favorites")]
public sealed class FavoritesController(FavoriteService favorites) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<FavoriteView>> List(CancellationToken cancellationToken) =>
        favorites.ListAsync(cancellationToken);

    [HttpPut("{locationId:guid}")]
    public async Task<IActionResult> Add(Guid locationId, CancellationToken cancellationToken)
    {
        await favorites.AddAsync(locationId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{locationId:guid}")]
    public async Task<IActionResult> Remove(Guid locationId, CancellationToken cancellationToken)
    {
        await favorites.RemoveAsync(locationId, cancellationToken);
        return NoContent();
    }
}
