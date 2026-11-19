using Npgsql;

namespace GjejVend.Infrastructure.IntegrationTests;

[Collection(DatabaseGroup.Name)]
public sealed class ReviewTests(DatabaseFixture database)
{
    private const string InsertReview =
        "INSERT INTO reviews (booking_id, customer_id, location_id, rating) SELECT id, customer_id, location_id, $2 FROM bookings WHERE id = $1";

    [Fact]
    public async Task Review_requires_completed_booking()
    {
        await using var connection = await database.OpenAsync();
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), await connection.CreateLocationAsync());

        Assert.Equal("review_booking_not_eligible", await connection.FailureAsync(InsertReview, booking, 5));
    }

    [Fact]
    public async Task Completed_booking_can_be_reviewed_once_and_updates_rating()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 2);
        var first = await CompleteBookingAsync(connection, location);
        var second = await CompleteBookingAsync(connection, location);

        await connection.ExecuteAsync(InsertReview, first, 5);
        await connection.ExecuteAsync(InsertReview, second, 4);
        var duplicate = await Assert.ThrowsAsync<PostgresException>(() => connection.ExecuteAsync(InsertReview, first, 1));

        Assert.Equal("23505", duplicate.SqlState);
        Assert.Equal(4.50m, await connection.ScalarAsync<decimal>("SELECT rating_average FROM parking_locations WHERE id = $1", location));
        Assert.Equal(2, await connection.ScalarAsync<int>("SELECT rating_count FROM parking_locations WHERE id = $1", location));
    }

    private static async Task<Guid> CompleteBookingAsync(NpgsqlConnection connection, Guid location)
    {
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), location);
        foreach (var status in new[] { "confirmed", "active", "completed" })
        {
            await connection.ExecuteAsync("UPDATE bookings SET status = $2 WHERE id = $1", booking, status);
        }

        return booking;
    }
}
