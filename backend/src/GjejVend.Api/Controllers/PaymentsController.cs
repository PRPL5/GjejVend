using GjejVend.Application.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(IPaymentService payments, IPaymentWebhookService webhook) : ControllerBase
{
    /// <summary>Starts payment for the caller's pending booking and returns the provider checkout data.</summary>
    [HttpPost("bookings/{bookingId:guid}/pay")]
    [Authorize]
    public async Task<ActionResult<PayBookingResult>> Pay(Guid bookingId, CancellationToken ct)
        => Ok(await payments.PayBookingAsync(bookingId, ct));

    /// <summary>Provider callback. Authenticated by its signature header, not by a JWT.</summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["X-Signature"].FirstOrDefault();
        return await webhook.HandleAsync(body, signature, ct) ? Ok() : Unauthorized();
    }
}
