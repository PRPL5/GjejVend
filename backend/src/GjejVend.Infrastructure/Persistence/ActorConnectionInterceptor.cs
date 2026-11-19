using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GjejVend.Infrastructure.Persistence;

public sealed class ActorConnectionInterceptor(ICurrentUser currentUser) : DbConnectionInterceptor
{
    private const string SetActorSql = "SELECT set_config('app.actor_id', @actor_id, false)";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var command = CreateSetActorCommand(connection);
        command?.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var command = CreateSetActorCommand(connection);
        if (command is not null)
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private DbCommand? CreateSetActorCommand(DbConnection connection)
    {
        if (currentUser.Id is not { } actorId)
        {
            return null;
        }

        var command = connection.CreateCommand();
        command.CommandText = SetActorSql;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "actor_id";
        parameter.Value = actorId.ToString();
        command.Parameters.Add(parameter);
        return command;
    }
}
