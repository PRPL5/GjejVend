using GjejVend.Application.Common;
using GjejVend.Domain.Moderation;
using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Admin;

public sealed record AuditLogQuery(
    Guid? ActorId, string? EntityType, Guid? EntityId, DateTime? From, DateTime? To, int Page = 1, int PageSize = 50);

public sealed record AuditLogItem(
    Guid Id, Guid? ActorId, string Action, string EntityType, Guid? EntityId, string Metadata, DateTime CreatedAt);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);

public interface IAuditLogService
{
    Task<PagedResult<AuditLogItem>> ListAsync(AuditLogQuery query, CancellationToken ct = default);
}

/// <summary>Read-only view over audit_logs. Rows are written by database triggers and cannot be edited.</summary>
public sealed class AuditLogService(GjejVendDbContext db, AccessGuard access) : IAuditLogService
{
    private const int MaxPageSize = 200;

    public async Task<PagedResult<AuditLogItem>> ListAsync(AuditLogQuery query, CancellationToken ct = default)
    {
        await access.RequireAdminAsync(ct);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var logs = db.Set<AuditLog>().AsNoTracking().AsQueryable();
        if (query.ActorId is { } actor) logs = logs.Where(l => l.ActorId == actor);
        if (!string.IsNullOrWhiteSpace(query.EntityType)) logs = logs.Where(l => l.EntityType == query.EntityType);
        if (query.EntityId is { } entity) logs = logs.Where(l => l.EntityId == entity);
        if (query.From is { } from) logs = logs.Where(l => l.CreatedAt >= from);
        if (query.To is { } to) logs = logs.Where(l => l.CreatedAt < to);

        var total = await logs.CountAsync(ct);
        var items = await logs.OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new AuditLogItem(l.Id, l.ActorId, l.Action, l.EntityType, l.EntityId, l.Metadata, l.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<AuditLogItem>(items, page, pageSize, total);
    }
}
