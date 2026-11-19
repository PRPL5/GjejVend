using GjejVend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.IntegrationTests;

[Collection(DatabaseGroup.Name)]
public sealed class ActorTests(DatabaseFixture database)
{
    [Fact]
    public async Task Current_user_is_recorded_as_actor_on_booking_history()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var location = await connection.CreateLocationAsync();
        await using var context = database.CreateContext(new FixedCurrentUser(customer));

        var booking = await context.Database
            .SqlQueryRaw<Guid>(
                "SELECT id AS \"Value\" FROM create_booking({0}, {1}, now() + interval '1 hour', now() + interval '2 hours', 'AB123CD', 'car')",
                customer,
                location)
            .SingleAsync();

        Assert.Equal(customer, await connection.ScalarAsync<Guid>("SELECT changed_by FROM booking_status_history WHERE booking_id = $1", booking));
    }

    [Fact]
    public async Task Anonymous_request_sets_no_actor()
    {
        await using var context = database.CreateContext(new FixedCurrentUser(null));

        var actor = await context.Database
            .SqlQueryRaw<string>("SELECT coalesce(current_setting('app.actor_id', true), '') AS \"Value\"")
            .SingleAsync();

        Assert.Equal(string.Empty, actor);
    }

    private sealed record FixedCurrentUser(Guid? Id) : ICurrentUser;
}
