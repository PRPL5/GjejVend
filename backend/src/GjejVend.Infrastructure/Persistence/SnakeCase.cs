using System.Globalization;
using System.Text.RegularExpressions;

namespace GjejVend.Infrastructure.Persistence;

public static partial class SnakeCase
{
    public static string From(string name) =>
        WordBoundary().Replace(name, "$1_$2").Replace("__", "_", StringComparison.Ordinal).ToLower(CultureInfo.InvariantCulture);

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex WordBoundary();
}
