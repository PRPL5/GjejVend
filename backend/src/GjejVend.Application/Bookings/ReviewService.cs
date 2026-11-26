using FluentValidation;
using GjejVend.Domain.Bookings;
using GjejVend.Domain.Common;
using GjejVend.Domain.Reviews;
using GjejVend.Infrastructure.Bookings;
using GjejVend.Infrastructure.Persistence;

namespace GjejVend.Application.Bookings;

public sealed class ReviewService(
    IReviewRepository reviews,
    IBookingRepository bookings,
    ICurrentUser currentUser,
    TimeProvider time,
    IValidator<ReviewRequest> reviewValidator,
    IValidator<ReviewResponseRequest> responseValidator)
{
    public async Task<LocationReviewView> AddAsync(Guid bookingId, ReviewRequest request, CancellationToken cancellationToken)
    {
        await reviewValidator.ValidateAndThrowAsync(request, cancellationToken);
        var booking = await bookings.FindAsync(bookingId, cancellationToken);
        if (booking is null || booking.CustomerId != currentUser.RequireId())
        {
            throw BookingService.BookingNotFound();
        }

        if (booking.Status != BookingStatus.Completed)
        {
            throw new ConflictException("review_booking_not_eligible", "Only completed bookings can be reviewed.");
        }

        var review = await reviews.AddAsync(
            new Review
            {
                BookingId = booking.Id,
                CustomerId = booking.CustomerId,
                LocationId = booking.LocationId,
                Rating = request.Rating,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            },
            cancellationToken);

        return new LocationReviewView(review.Id, review.Rating, review.Comment, review.ProviderResponse, review.CreatedAt);
    }

    public async Task<LocationReviewView> RespondAsync(Guid reviewId, ReviewResponseRequest request, CancellationToken cancellationToken)
    {
        await responseValidator.ValidateAndThrowAsync(request, cancellationToken);
        var review = await reviews.FindAsync(reviewId, cancellationToken)
            ?? throw new NotFoundException("review_not_found", "Review not found.");

        if (await bookings.FindLocationOwnerAsync(review.LocationId, cancellationToken) != currentUser.RequireId())
        {
            throw new ForbiddenException("not_location_owner", "Only the owner of this parking can reply.");
        }

        review.ProviderResponse = request.Response.Trim();
        review.RespondedAt = time.GetUtcNow().UtcDateTime;
        await reviews.SaveAsync(cancellationToken);

        return new LocationReviewView(review.Id, review.Rating, review.Comment, review.ProviderResponse, review.CreatedAt);
    }

    public Task<IReadOnlyList<OwnerReviewView>> ListForOwnerAsync(CancellationToken cancellationToken) =>
        reviews.ListForOwnerAsync(currentUser.RequireId(), cancellationToken);

    public Task<IReadOnlyList<LocationReviewView>> ListForLocationAsync(Guid locationId, CancellationToken cancellationToken) =>
        reviews.ListForLocationAsync(locationId, cancellationToken);
}
