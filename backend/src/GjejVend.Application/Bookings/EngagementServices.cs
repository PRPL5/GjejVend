using GjejVend.Application.Common;
using GjejVend.Domain.Common;
using GjejVend.Infrastructure.Bookings;
using GjejVend.Infrastructure.Persistence;

namespace GjejVend.Application.Bookings;

public sealed record NotificationView(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? EntityType,
    Guid? EntityId,
    DateTime? ReadAt,
    DateTime CreatedAt);

public sealed record NotificationList(IReadOnlyList<NotificationView> Items, int UnreadCount);

public sealed class FavoriteService(IFavoriteRepository favorites, ICurrentUser currentUser)
{
    public Task<IReadOnlyList<FavoriteView>> ListAsync(CancellationToken cancellationToken) =>
        favorites.ListAsync(currentUser.RequireId(), cancellationToken);

    public async Task AddAsync(Guid locationId, CancellationToken cancellationToken)
    {
        if (!await favorites.LocationExistsAsync(locationId, cancellationToken))
        {
            throw new NotFoundException("location_not_found", "Parking not found.");
        }

        await favorites.AddAsync(currentUser.RequireId(), locationId, cancellationToken);
    }

    public Task RemoveAsync(Guid locationId, CancellationToken cancellationToken) =>
        favorites.RemoveAsync(currentUser.RequireId(), locationId, cancellationToken);
}

public sealed class NotificationService(INotificationRepository notifications, ICurrentUser currentUser, TimeProvider time)
{
    private const int PageSize = 50;

    public async Task<NotificationList> ListAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireId();
        var items = await notifications.ListAsync(userId, PageSize, cancellationToken);
        var unread = await notifications.CountUnreadAsync(userId, cancellationToken);

        return new NotificationList(
            items.Select(n => new NotificationView(n.Id, n.Type, n.Title, n.Body, n.EntityType, n.EntityId, n.ReadAt, n.CreatedAt)).ToList(),
            unread);
    }

    public async Task MarkReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        if (await notifications.MarkReadAsync(currentUser.RequireId(), notificationId, time.GetUtcNow().UtcDateTime, cancellationToken) == 0)
        {
            throw new NotFoundException("notification_not_found", "Notification not found or already read.");
        }
    }

    public Task MarkAllReadAsync(CancellationToken cancellationToken) =>
        notifications.MarkReadAsync(currentUser.RequireId(), null, time.GetUtcNow().UtcDateTime, cancellationToken);
}
