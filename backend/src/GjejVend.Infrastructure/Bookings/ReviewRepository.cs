using GjejVend.Domain.Listings;
using GjejVend.Domain.Reviews;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Bookings;

public interface IReviewRepository
{
    Task<Review> AddAsync(Review review, CancellationToken cancellationToken);

    Task<Review?> FindAsync(Guid reviewId, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<OwnerReviewView>> ListForOwnerAsync(Guid ownerId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LocationReviewView>> ListForLocationAsync(Guid locationId, CancellationToken cancellationToken);
}

public sealed record OwnerReviewView(
    Guid Id,
    Guid LocationId,
    string LocationName,
    int Rating,
    string? Comment,
    string? ProviderResponse,
    DateTime? RespondedAt,
    DateTime CreatedAt);

public sealed record LocationReviewView(
    Guid Id,
    int Rating,
    string? Comment,
    string? ProviderResponse,
    DateTime CreatedAt);

internal sealed class ReviewRepository(GjejVendDbContext context) : IReviewRepository
{
    private IQueryable<Review> Reviews => context.Set<Review>();

    public async Task<Review> AddAsync(Review review, CancellationToken cancellationToken)
    {
        context.Add(review);
        try
        {
            await DatabaseErrors.GuardAsync(() => context.SaveChangesAsync(cancellationToken));
            return review;
        }
        catch
        {
            context.Entry(review).State = EntityState.Detached;
            throw;
        }
    }

    public Task<Review?> FindAsync(Guid reviewId, CancellationToken cancellationToken) =>
        Reviews.SingleOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken) =>
        DatabaseErrors.GuardAsync(() => context.SaveChangesAsync(cancellationToken));

    public async Task<IReadOnlyList<OwnerReviewView>> ListForOwnerAsync(Guid ownerId, CancellationToken cancellationToken) =>
        await Reviews
            .Join(context.Set<ParkingLocation>(), r => r.LocationId, l => l.Id, (r, l) => new { Review = r, Location = l })
            .Join(context.Set<ParkingProvider>().Where(p => p.OwnerId == ownerId), x => x.Location.ProviderId, p => p.Id, (x, _) => x)
            .OrderByDescending(x => x.Review.CreatedAt)
            .Take(200)
            .Select(x => new OwnerReviewView(
                x.Review.Id,
                x.Location.Id,
                x.Location.Name,
                x.Review.Rating,
                x.Review.Comment,
                x.Review.ProviderResponse,
                x.Review.RespondedAt,
                x.Review.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LocationReviewView>> ListForLocationAsync(Guid locationId, CancellationToken cancellationToken) =>
        await Reviews
            .Where(r => r.LocationId == locationId && r.Status == ReviewStatus.Published)
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .Select(r => new LocationReviewView(r.Id, r.Rating, r.Comment, r.ProviderResponse, r.CreatedAt))
            .ToListAsync(cancellationToken);
}
