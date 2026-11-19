using GjejVend.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence.Configurations.Payments;

internal sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds", table => table.HasCheckConstraint("ck_refunds_amount", "amount > 0"));
        builder.HasTimestamps();
        builder.Property(r => r.Status).DefaultsTo(RefundStatus.Pending);
        builder.HasOne<Payment>().WithMany().HasForeignKey(r => r.PaymentId);
        builder.HasIndex(r => r.PaymentId);
    }
}
