using GjejVend.Domain.Admin;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Application.Common;

public interface IClock
{
    DateTime UtcNow { get; }
}

public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsInRole(string role);
}

public static class Roles
{
    public const string Customer = "customer";
    public const string Provider = "provider";
    public const string Admin = "admin";
}

public interface IApplicationDbContext
{
    DbSet<Payment> Payments { get; }
    DbSet<Refund> Refunds { get; }
    DbSet<ProviderPayout> ProviderPayouts { get; }
    DbSet<LedgerEntry> LedgerEntries { get; }
    DbSet<PlatformSetting> PlatformSettings { get; }
    DbSet<Report> Reports { get; }
    DbSet<VerificationRequest> VerificationRequests { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public class NotFoundException(string message) : Exception(message);

public class ForbiddenException(string message) : Exception(message);

public class ValidationFailedException(string message) : Exception(message);
