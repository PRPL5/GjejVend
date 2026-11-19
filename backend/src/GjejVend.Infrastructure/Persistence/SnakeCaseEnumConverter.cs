using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GjejVend.Infrastructure.Persistence;

public sealed class SnakeCaseEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
    value => SnakeCase.From(value.ToString()),
    value => Enum.Parse<TEnum>(value.Replace("_", string.Empty, StringComparison.Ordinal), true))
    where TEnum : struct, Enum;
