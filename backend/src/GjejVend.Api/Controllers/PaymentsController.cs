using GjejVend.Application.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(
    IPaymentService payments, IPaymentWebhookService webhook, IRefundService refunds, IInvoiceService invoices) : ControllerBase
{
    /// <summary>Starts payment for the caller's pending booking and returns the provider checkout data.</summary>
    [HttpPost("bookings/{bookingId:guid}/pay")]
    [Authorize]
    public async Task<ActionResult<PayBookingResult>> Pay(Guid bookingId, CancellationToken ct)
        => Ok(await payments.PayBookingAsync(bookingId, ct));

    /// <summary>What the caller would be refunded if they cancelled the booking now.</summary>
    [HttpGet("bookings/{bookingId:guid}/refund-preview")]
    [Authorize]
    public async Task<ActionResult<RefundPreview>> RefundPreview(Guid bookingId, CancellationToken ct)
        => Ok(await refunds.PreviewAsync(bookingId, ct));

    /// <summary>The caller's paid bookings, for the invoices list in My bookings.</summary>
    [HttpGet("invoices")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<PaidBookingSummary>>> ListInvoices(CancellationToken ct)
        => Ok(await invoices.ListMineAsync(ct));

    /// <summary>The Stripe-hosted invoice and PDF links for one paid booking.</summary>
    [HttpGet("bookings/{bookingId:guid}/invoice")]
    [Authorize]
    public async Task<ActionResult<BookingInvoice>> GetInvoice(Guid bookingId, CancellationToken ct)
        => Ok(await invoices.GetForBookingAsync(bookingId, ct));

    /// <summary>Stripe callback (the fake gateway uses X-Signature). Authenticated by signature, not by a JWT.</summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault()
                        ?? Request.Headers["X-Signature"].FirstOrDefault();
        return await webhook.HandleAsync(body, signature, ct) ? Ok() : Unauthorized();
    }
}
