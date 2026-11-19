namespace GjejVend.Infrastructure.IntegrationTests;

[Collection(DatabaseGroup.Name)]
public sealed class EngagementTests(DatabaseFixture database)
{
    [Fact]
    public async Task Favorites_are_unique_per_user_and_location()
    {
        await using var connection = await database.OpenAsync();
        var user = await connection.CreateUserAsync();
        var location = await connection.CreateLocationAsync();
        const string insert = "INSERT INTO favorite_locations (user_id, location_id) VALUES ($1, $2)";

        await connection.ExecuteAsync(insert, user, location);
        var duplicate = await Assert.ThrowsAsync<Npgsql.PostgresException>(() => connection.ExecuteAsync(insert, user, location));

        Assert.Equal("23505", duplicate.SqlState);
    }

    [Fact]
    public async Task Notification_is_stored_unread()
    {
        await using var connection = await database.OpenAsync();
        var user = await connection.CreateUserAsync();

        await connection.ExecuteAsync("INSERT INTO notifications (user_id, type, title, body) VALUES ($1, 'booking_confirmed', 'Confirmed', 'See you soon')", user);

        Assert.Equal(1L, await connection.ScalarAsync<long>("SELECT count(*) FROM notifications WHERE user_id = $1 AND read_at IS NULL", user));
    }
}
