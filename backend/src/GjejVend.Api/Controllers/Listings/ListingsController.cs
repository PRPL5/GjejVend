using GjejVend.Application.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers.Listings;

[ApiController]
[Authorize]
[Route("api/listings")]
public sealed class ListingsController(IListingService listings) : ControllerBase
{
    /// <summary>Create a draft lot for the signed-in provider. Returns 201 with the new lot.</summary>
    [HttpPost]
    public async Task<ActionResult<ListingResponse>> Create(ListingRequest request, CancellationToken ct)
    {
        var lot = await listings.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetMine), new { id = lot.Id }, lot);
    }

    /// <summary>All lots of the signed-in provider, newest first.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<ListingResponse>>> ListMine(CancellationToken ct) =>
        Ok(await listings.ListMineAsync(ct));

    /// <summary>One of the signed-in provider's lots. 403 for another owner's lot, 404 when missing.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ListingResponse>> GetMine(Guid id, CancellationToken ct) =>
        Ok(await listings.GetMineAsync(id, ct));

    /// <summary>Edit one of the signed-in provider's lots.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ListingResponse>> Update(Guid id, ListingRequest request, CancellationToken ct) =>
        Ok(await listings.UpdateAsync(id, request, ct));

    /// <summary>Soft-delete one of the signed-in provider's lots. Returns 204.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await listings.DeleteAsync(id, ct);
        return NoContent();
    }
}
