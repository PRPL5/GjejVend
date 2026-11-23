using GjejVend.Api.Extensions;
using GjejVend.Application.Common;
using GjejVend.Application.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers;

[ApiController]
[Route("api/owner/earnings")]
[Authorize(Policy = AuthExtensions.ProviderPolicy)]
public sealed class OwnerEarningsController(ILedgerService ledger, ICurrentUser user, IClock clock) : ControllerBase
{
    /// <summary>Earnings for the signed-in provider. Defaults to the last 7 days.</summary>
    [HttpGet]
    public async Task<ActionResult<EarningsSummary>> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var providerId = user.UserId ?? throw new ForbiddenException("Sign in required.");
        var end = to ?? clock.UtcNow;
        var start = from ?? end.AddDays(-7);
        if (start >= end) throw new ValidationFailedException("'from' must be before 'to'.");
        return Ok(await ledger.GetProviderEarningsAsync(providerId, start, end, ct));
    }
}
