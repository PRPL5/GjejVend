using GjejVend.Application.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/owner/earnings")]
public sealed class OwnerEarningsController(IEarningsService earnings, TimeProvider clock) : ControllerBase
{
    /// <summary>Earnings of the signed-in provider. Defaults to the last 7 days.</summary>
    [HttpGet]
    public async Task<ActionResult<EarningsSummary>> Get(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var end = to ?? clock.GetUtcNow().UtcDateTime;
        var start = from ?? end.AddDays(-7);
        return Ok(await earnings.GetOwnerEarningsAsync(start, end, ct));
    }
}
