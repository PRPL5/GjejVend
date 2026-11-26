using GjejVend.Application.Bookings;
using GjejVend.Infrastructure.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers.Bookings;

[ApiController]
[Authorize]
[Route("owner")]
public sealed class OwnerController(BookingDeskService desk, ReviewService reviews) : ControllerBase
{
    [HttpGet("arrivals")]
    public Task<IReadOnlyList<ArrivalView>> Arrivals([FromQuery] ArrivalsQuery query, CancellationToken cancellationToken) =>
        desk.ListArrivalsAsync(query, cancellationToken);

    [HttpGet("reviews")]
    public Task<IReadOnlyList<OwnerReviewView>> Reviews(CancellationToken cancellationToken) =>
        reviews.ListForOwnerAsync(cancellationToken);
}
