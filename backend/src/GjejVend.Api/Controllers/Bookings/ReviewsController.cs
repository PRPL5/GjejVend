using GjejVend.Application.Bookings;
using GjejVend.Infrastructure.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GjejVend.Api.Controllers.Bookings;

[ApiController]
[Route("reviews")]
public sealed class ReviewsController(ReviewService reviews) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<LocationReviewView>> ListForLocation([FromQuery] Guid locationId, CancellationToken cancellationToken) =>
        reviews.ListForLocationAsync(locationId, cancellationToken);

    [Authorize]
    [HttpPost("{id:guid}/response")]
    public Task<LocationReviewView> Respond(Guid id, ReviewResponseRequest request, CancellationToken cancellationToken) =>
        reviews.RespondAsync(id, request, cancellationToken);
}
