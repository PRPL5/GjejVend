using GjejVend.Application.Bookings;
using GjejVend.Infrastructure.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers.Bookings;

[ApiController]
[Authorize]
[Route("bookings")]
public sealed class BookingsController(BookingService bookings, BookingDeskService desk, ReviewService reviews) : ControllerBase
{
    [HttpGet("preview")]
    public Task<CheckoutPreview> Preview([FromQuery] CheckoutPreviewQuery query, CancellationToken cancellationToken) =>
        bookings.PreviewAsync(query, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<TicketView>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var ticket = await bookings.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ticket);
    }

    [HttpGet]
    public Task<IReadOnlyList<BookingSummary>> ListMine(CancellationToken cancellationToken) =>
        bookings.ListMineAsync(cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<TicketView> Get(Guid id, CancellationToken cancellationToken) =>
        bookings.GetTicketAsync(id, cancellationToken);

    [HttpPost("{id:guid}/extend")]
    public Task<TicketView> Extend(Guid id, ExtendBookingRequest request, CancellationToken cancellationToken) =>
        bookings.ExtendAsync(id, request, cancellationToken);

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancelBookingRequest request, CancellationToken cancellationToken)
    {
        await bookings.CancelAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/review")]
    public async Task<ActionResult<LocationReviewView>> Review(Guid id, ReviewRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await reviews.AddAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/check-in")]
    public Task<ArrivalView> CheckIn(Guid id, CancellationToken cancellationToken) =>
        desk.CheckInAsync(id, cancellationToken);

    [HttpPost("{id:guid}/check-out")]
    public Task<ArrivalView> CheckOut(Guid id, CancellationToken cancellationToken) =>
        desk.CheckOutAsync(id, cancellationToken);

    [HttpPost("check-in")]
    public Task<ArrivalView> CheckInByTicket(TicketScanRequest request, CancellationToken cancellationToken) =>
        desk.CheckInByTicketAsync(request, cancellationToken);

    [HttpPost("check-out")]
    public Task<ArrivalView> CheckOutByTicket(TicketScanRequest request, CancellationToken cancellationToken) =>
        desk.CheckOutByTicketAsync(request, cancellationToken);
}
