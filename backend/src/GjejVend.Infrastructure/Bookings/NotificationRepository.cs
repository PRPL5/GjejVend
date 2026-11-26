using GjejVend.Domain.Engagement;
using GjejVend.Domain.Identity;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Bookings;

public interface INotificationRepository
{
    Task AddAsync(IReadOnlyCollection<Notification> notifications, CancellationToken cancellationToken);

    Task<IReadOnlyList<Notification>> ListAsync(Guid userId, int limit, CancellationToken cancellationToken);

    Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken);

    Task<int> MarkReadAsync(Guid userId, Guid? notificationId, DateTime at, CancellationToken cancellationToken);

    Task<NotificationContact?> FindContactAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed record NotificationContact(string? Email, bool NotifyEmail, bool NotifyBooking);

internal sealed class NotificationRepository(GjejVendDbContext context) : INotificationRepository
{
    private IQueryable<Notification> Notifications => context.Set<Notification>();

    public async Task AddAsync(IReadOnlyCollection<Notification> notifications, CancellationToken cancellationToken)
    {
        context.AddRange(notifications);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            foreach (var notification in notifications)
            {
                context.Entry(notification).State = EntityState.Detached;
            }
        }
    }

    public async Task<IReadOnlyList<Notification>> ListAsync(Guid userId, int limit, CancellationToken cancellationToken) =>
        await Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken) =>
        Notifications.CountAsync(n => n.UserId == userId && n.ReadAt == null, cancellationToken);

    public Task<int> MarkReadAsync(Guid userId, Guid? notificationId, DateTime at, CancellationToken cancellationToken) =>
        Notifications
            .Where(n => n.UserId == userId && n.ReadAt == null && (notificationId == null || n.Id == notificationId))
            .ExecuteUpdateAsync(set => set.SetProperty(n => n.ReadAt, at), cancellationToken);

    public Task<NotificationContact?> FindContactAsync(Guid userId, CancellationToken cancellationToken) =>
        context.Set<Profile>()
            .Where(p => p.Id == userId && p.Status == AccountStatus.Active)
            .Select(p => new NotificationContact(p.Email, p.NotifyEmail, p.NotifyBooking))
            .SingleOrDefaultAsync(cancellationToken);
}
