using GjejVend.Application.Payments;
using GjejVend.Domain.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers;

public sealed record OnboardingRequest(Guid? ProviderId);

public sealed record RunPayoutsRequest(DateOnly PeriodStart, DateOnly PeriodEnd);

[ApiController]
[Authorize]
[Route("api/owner")]
public sealed class OwnerPayoutsController(IPayoutService payouts) : ControllerBase
{
    /// <summary>Returns a link to Stripe's hosted onboarding so the owner can receive payouts.</summary>
    [HttpPost("stripe/onboarding")]
    public async Task<ActionResult<OnboardingLink>> StartOnboarding([FromBody] OnboardingRequest? request, CancellationToken ct)
        => Ok(await payouts.StartOnboardingAsync(request?.ProviderId, ct));

    [HttpGet("payouts")]
    public async Task<ActionResult<IReadOnlyList<PayoutDto>>> List(CancellationToken ct)
        => Ok(await payouts.ListMineAsync(ct));
}

[ApiController]
[Authorize]
[Route("api/admin/payouts")]
public sealed class AdminPayoutsController(IPayoutService payouts) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PayoutDto>>> List([FromQuery] PayoutStatus? status, CancellationToken ct)
        => Ok(await payouts.ListAsync(status, ct));

    /// <summary>Pays every provider with net earnings in the period. Re-running the same period is safe.</summary>
    [HttpPost("run")]
    public async Task<ActionResult<IReadOnlyList<PayoutRunItem>>> Run([FromBody] RunPayoutsRequest request, CancellationToken ct)
        => Ok(await payouts.RunAsync(request.PeriodStart, request.PeriodEnd, ct));
}
