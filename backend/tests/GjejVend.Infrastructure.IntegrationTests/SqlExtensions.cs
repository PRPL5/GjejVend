using Npgsql;

namespace GjejVend.Infrastructure.IntegrationTests;

internal static class SqlExtensions
{
    public static async Task<T> ScalarAsync<T>(this NpgsqlConnection connection, string sql, params object[] arguments)
    {
        await using var command = CreateCommand(connection, sql, arguments);
        return (T)(await command.ExecuteScalarAsync())!;
    }

    public static async Task ExecuteAsync(this NpgsqlConnection connection, string sql, params object[] arguments)
    {
        await using var command = CreateCommand(connection, sql, arguments);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<string> FailureAsync(this NpgsqlConnection connection, string sql, params object[] arguments)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(() => connection.ExecuteAsync(sql, arguments));
        return exception.MessageText;
    }

    private static NpgsqlCommand CreateCommand(NpgsqlConnection connection, string sql, object[] arguments)
    {
        var command = new NpgsqlCommand(sql, connection);
        foreach (var argument in arguments)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = argument });
        }

        return command;
    }
}
