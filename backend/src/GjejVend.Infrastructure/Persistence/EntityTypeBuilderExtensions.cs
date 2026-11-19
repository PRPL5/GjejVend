using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GjejVend.Infrastructure.Persistence;

internal static class EntityTypeBuilderExtensions
{
    public static PropertyBuilder<DateTime> HasCreatedAt<TEntity>(this EntityTypeBuilder<TEntity> builder, string name = "CreatedAt")
        where TEntity : class =>
        builder.Property<DateTime>(name).HasDefaultValueSql("now()");

    public static void HasTimestamps<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.HasCreatedAt();
        builder.HasCreatedAt("UpdatedAt");
    }

    public static PropertyBuilder<string> IsCurrency(this PropertyBuilder<string> builder) =>
        builder.HasColumnType("char(3)").DefaultsTo("EUR");

    public static PropertyBuilder<string> IsJson(this PropertyBuilder<string> builder) =>
        builder.HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb").ValueGeneratedNever();

    public static PropertyBuilder<T> DefaultsTo<T>(this PropertyBuilder<T> builder, T value) =>
        builder.HasDefaultValue(value).ValueGeneratedNever();
}
