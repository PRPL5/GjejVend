using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GjejVend.Infrastructure.Persistence;

internal static class ModelBuilderConventions
{
    public static void ApplyDatabaseConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(SnakeCase.From(property.Name));
                ApplyGeneratedUuid(property);
                ApplyEnumMapping(entity, property);
                ApplyFalseDefault(property);
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(SnakeCase.From(key.GetDefaultName()!));
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(SnakeCase.From(foreignKey.GetDefaultName()!));
            }

            foreach (var index in entity.GetIndexes().Where(i => i.GetDatabaseName() == i.GetDefaultDatabaseName()))
            {
                index.SetDatabaseName(SnakeCase.From(index.GetDefaultDatabaseName()!));
            }
        }
    }

    private static void ApplyFalseDefault(IMutableProperty property)
    {
        if (property.ClrType != typeof(bool) || property.FindAnnotation(RelationalAnnotationNames.DefaultValue) is not null)
        {
            return;
        }

        property.SetDefaultValue(false);
        property.ValueGenerated = ValueGenerated.Never;
    }

    private static void ApplyGeneratedUuid(IMutableProperty property)
    {
        if (property.IsPrimaryKey() && property.ClrType == typeof(Guid) && property.ValueGenerated == ValueGenerated.OnAdd)
        {
            property.SetDefaultValueSql("gen_random_uuid()");
        }
    }

    private static void ApplyEnumMapping(IMutableEntityType entity, IMutableProperty property)
    {
        var enumType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
        if (!enumType.IsEnum)
        {
            return;
        }

        var converterType = typeof(SnakeCaseEnumConverter<>).MakeGenericType(enumType);
        property.SetValueConverter((ValueConverter)Activator.CreateInstance(converterType)!);

        var column = property.GetColumnName();
        var allowed = string.Join(", ", Enum.GetNames(enumType).Select(name => $"'{SnakeCase.From(name)}'"));
        entity.AddCheckConstraint($"ck_{entity.GetTableName()}_{column}", $"{column} IN ({allowed})");
    }
}
