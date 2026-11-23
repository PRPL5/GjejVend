using GjejVend.Application.Common;
using GjejVend.Domain.Admin;
using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<ProviderPayout> ProviderPayouts => Set<ProviderPayout>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<PlatformSetting> PlatformSettings => Set<PlatformSetting>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<VerificationRequest> VerificationRequests => Set<VerificationRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(12, 2);
            e.Property(x => x.RefundedAmount).HasPrecision(12, 2);
            e.Property(x => x.Currency).HasMaxLength(3);
            e.Property(x => x.PaymentProvider).HasMaxLength(50);
            e.Property(x => x.ProviderReference).HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.BookingId);
            e.HasIndex(x => x.ProviderReference).IsUnique();
        });

        modelBuilder.Entity<Refund>(e =>
        {
            e.ToTable("refunds");
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(12, 2);
            e.Property(x => x.Reason).HasMaxLength(500);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne<Payment>().WithMany().HasForeignKey(x => x.PaymentId);
        });

        modelBuilder.Entity<ProviderPayout>(e =>
        {
            e.ToTable("provider_payouts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(12, 2);
            e.Property(x => x.Currency).HasMaxLength(3);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => new { x.ProviderId, x.PeriodFrom, x.PeriodTo });
        });

        modelBuilder.Entity<LedgerEntry>(e =>
        {
            e.ToTable("ledger_entries");
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(12, 2);
            e.Property(x => x.Currency).HasMaxLength(3);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            e.HasIndex(x => new { x.ProviderId, x.CreatedAt });
            e.HasIndex(x => x.BookingId);
        });

        modelBuilder.Entity<PlatformSetting>(e =>
        {
            e.ToTable("platform_settings");
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(100);
            e.Property(x => x.Value).HasMaxLength(1000);
        });

        modelBuilder.Entity<Report>(e =>
        {
            e.ToTable("reports");
            e.HasKey(x => x.Id);
            e.Property(x => x.TargetType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Category).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<VerificationRequest>(e =>
        {
            e.ToTable("verification_requests");
            e.HasKey(x => x.Id);
            e.Property(x => x.TargetType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(100);
            e.Property(x => x.EntityType).HasMaxLength(50);
            e.HasIndex(x => x.CreatedAt);
        });
    }
}
