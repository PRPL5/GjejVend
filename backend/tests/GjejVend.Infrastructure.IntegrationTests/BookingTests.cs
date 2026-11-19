namespace GjejVend.Infrastructure.IntegrationTests;

[Collection(DatabaseGroup.Name)]
public sealed class BookingTests(DatabaseFixture database)
{
    [Fact]
    public async Task Booking_is_priced_with_commission_and_starts_pending()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var location = await connection.CreateLocationAsync();

        var booking = await connection.BookAsync(customer, location);

        Assert.Equal("pending", await connection.ScalarAsync<string>("SELECT status FROM bookings WHERE id = $1", booking));
        Assert.Equal(4.50m, await connection.ScalarAsync<decimal>("SELECT total_amount FROM bookings WHERE id = $1", booking));
        Assert.Equal(0.45m, await connection.ScalarAsync<decimal>("SELECT commission_amount FROM bookings WHERE id = $1", booking));
        Assert.Equal("pending", await connection.ScalarAsync<string>("SELECT to_status FROM booking_status_history WHERE booking_id = $1", booking));
    }

    [Fact]
    public async Task Daily_rate_caps_long_bookings()
    {
        await using var connection = await database.OpenAsync();
        var customer = await connection.CreateUserAsync();
        var location = await connection.CreateLocationAsync();

        var booking = await connection.BookAsync(customer, location, 1, 29);

        Assert.Equal(16.00m, await connection.ScalarAsync<decimal>("SELECT total_amount FROM bookings WHERE id = $1", booking));
    }

    [Fact]
    public async Task Booking_beyond_capacity_is_rejected()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 1);
        await connection.BookAsync(await connection.CreateUserAsync(), location, 1, 4);

        var error = await connection.FailureAsync(TestData.BookSql(3, 5), await connection.CreateUserAsync(), location);

        Assert.Equal("booking_no_capacity", error);
    }

    [Fact]
    public async Task Offline_occupancy_reduces_capacity()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 2);
        await connection.ExecuteAsync("UPDATE parking_locations SET offline_occupied = 2 WHERE id = $1", location);

        var error = await connection.FailureAsync(TestData.BookSql(1, 2), await connection.CreateUserAsync(), location);

        Assert.Equal("booking_no_capacity", error);
    }

    [Fact]
    public async Task Concurrent_bookings_never_exceed_capacity()
    {
        await using var setup = await database.OpenAsync();
        var location = await setup.CreateLocationAsync(capacity: 3);
        var customers = new List<Guid>();
        for (var i = 0; i < 10; i++)
        {
            customers.Add(await setup.CreateUserAsync());
        }

        var results = await Task.WhenAll(customers.Select(async customer =>
        {
            await using var connection = await database.OpenAsync();
            try
            {
                await connection.BookAsync(customer, location);
                return true;
            }
            catch (Npgsql.PostgresException)
            {
                return false;
            }
        }));

        Assert.Equal(3, results.Count(succeeded => succeeded));
        Assert.Equal(3L, await setup.ScalarAsync<long>("SELECT count(*) FROM bookings WHERE location_id = $1", location));
    }

    [Fact]
    public async Task Concurrent_bookings_for_one_space_allow_only_one()
    {
        await using var setup = await database.OpenAsync();
        var location = await setup.CreateLocationAsync(inventoryMode: "spaces");
        var space = await setup.AddSpaceAsync(location);
        var customers = new List<Guid>();
        for (var i = 0; i < 5; i++)
        {
            customers.Add(await setup.CreateUserAsync());
        }

        var results = await Task.WhenAll(customers.Select(async customer =>
        {
            await using var connection = await database.OpenAsync();
            try
            {
                await connection.BookAsync(customer, location, spaceId: space);
                return true;
            }
            catch (Npgsql.PostgresException)
            {
                return false;
            }
        }));

        Assert.Equal(1, results.Count(succeeded => succeeded));
    }

    [Fact]
    public async Task Space_overlap_is_blocked_by_exclusion_constraint()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(inventoryMode: "spaces");
        var space = await connection.AddSpaceAsync(location);
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), location, spaceId: space);

        var exception = await Assert.ThrowsAsync<Npgsql.PostgresException>(() => connection.ExecuteAsync(
            """
            INSERT INTO bookings (customer_id, location_id, space_id, vehicle_plate, vehicle_type, starts_at, ends_at, status, expires_at, total_amount, commission_rate, commission_amount, provider_amount)
            SELECT customer_id, location_id, space_id, 'X', vehicle_type, starts_at, ends_at, 'pending', expires_at, 0, 0, 0, 0 FROM bookings WHERE id = $1
            """,
            booking));

        Assert.Equal("23P01", exception.SqlState);
    }

    [Fact]
    public async Task Expired_pending_booking_releases_capacity()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 1);
        var stale = await connection.BookAsync(await connection.CreateUserAsync(), location);
        await connection.ExecuteAsync("UPDATE bookings SET expires_at = now() - interval '1 minute' WHERE id = $1", stale);

        await connection.BookAsync(await connection.CreateUserAsync(), location);

        Assert.Equal("expired", await connection.ScalarAsync<string>("SELECT status FROM bookings WHERE id = $1", stale));
    }

    [Fact]
    public async Task Booking_outside_opening_hours_is_rejected()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(opensAt: "08:00", closesAt: "18:00");

        var error = await connection.FailureAsync(
            """
            SELECT create_booking($1, $2,
                (current_date + 2 + time '03:00') AT TIME ZONE 'Europe/Belgrade',
                (current_date + 2 + time '05:00') AT TIME ZONE 'Europe/Belgrade',
                'AB123CD', 'car')
            """,
            await connection.CreateUserAsync(), location);

        Assert.Equal("booking_outside_opening_hours", error);
    }

    [Fact]
    public async Task Booking_inside_local_opening_hours_succeeds()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(opensAt: "08:00", closesAt: "18:00");

        await connection.ExecuteAsync(
            """
            SELECT create_booking($1, $2,
                (current_date + 2 + time '09:00') AT TIME ZONE 'Europe/Belgrade',
                (current_date + 2 + time '17:00') AT TIME ZONE 'Europe/Belgrade',
                'AB123CD', 'car')
            """,
            await connection.CreateUserAsync(), location);
    }

    [Fact]
    public async Task Closed_override_blocks_booking()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync();
        await connection.ExecuteAsync(
            "INSERT INTO availability_overrides (location_id, starts_at, ends_at, kind) VALUES ($1, now(), now() + interval '1 day', 'closed')", location);

        var error = await connection.FailureAsync(TestData.BookSql(2, 3), await connection.CreateUserAsync(), location);

        Assert.Equal("booking_outside_opening_hours", error);
    }

    [Fact]
    public async Task Cancellation_frees_capacity_and_records_history()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 1);
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), location);

        await connection.ExecuteAsync("UPDATE bookings SET status = 'cancelled', cancellation_reason = 'plans changed' WHERE id = $1", booking);
        await connection.BookAsync(await connection.CreateUserAsync(), location);

        Assert.Equal("plans changed", await connection.ScalarAsync<string>(
            "SELECT reason FROM booking_status_history WHERE booking_id = $1 AND to_status = 'cancelled'", booking));
        Assert.True(await connection.ScalarAsync<bool>("SELECT cancelled_at IS NOT NULL FROM bookings WHERE id = $1", booking));
    }

    [Fact]
    public async Task Invalid_status_transition_is_rejected()
    {
        await using var connection = await database.OpenAsync();
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), await connection.CreateLocationAsync());
        await connection.ExecuteAsync("UPDATE bookings SET status = 'cancelled' WHERE id = $1", booking);

        var error = await connection.FailureAsync("UPDATE bookings SET status = 'confirmed' WHERE id = $1", booking);

        Assert.StartsWith("booking_invalid_transition", error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Extension_reprices_and_respects_capacity()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 1);
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), location, 1, 2);
        await connection.ExecuteAsync("UPDATE bookings SET status = 'confirmed' WHERE id = $1", booking);
        await connection.BookAsync(await connection.CreateUserAsync(), location, 3, 4);

        await connection.ExecuteAsync("SELECT extend_booking($1, (SELECT starts_at + interval '2 hours' FROM bookings WHERE id = $1))", booking);
        var error = await connection.FailureAsync("SELECT extend_booking($1, (SELECT starts_at + interval '3 hours' FROM bookings WHERE id = $1))", booking);

        Assert.Equal(3.00m, await connection.ScalarAsync<decimal>("SELECT total_amount FROM bookings WHERE id = $1", booking));
        Assert.Equal("booking_no_capacity", error);
    }

    [Fact]
    public async Task Bookings_cannot_be_deleted()
    {
        await using var connection = await database.OpenAsync();
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), await connection.CreateLocationAsync());

        var error = await connection.FailureAsync("DELETE FROM bookings WHERE id = $1", booking);

        Assert.Equal("DELETE on bookings is not allowed", error);
    }

    [Fact]
    public async Task Extension_expires_stale_pending_booking_on_the_same_space()
    {
        await using var connection = await database.OpenAsync();
        var location = await connection.CreateLocationAsync(inventoryMode: "spaces");
        var space = await connection.AddSpaceAsync(location);
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), location, 1, 2, space);
        await connection.ExecuteAsync("UPDATE bookings SET status = 'confirmed' WHERE id = $1", booking);
        var stale = await connection.BookAsync(await connection.CreateUserAsync(), location, 2, 3, space);
        await connection.ExecuteAsync("UPDATE bookings SET expires_at = now() - interval '1 minute' WHERE id = $1", stale);

        await connection.ExecuteAsync("SELECT extend_booking($1, (SELECT starts_at + interval '2 hours' FROM bookings WHERE id = $1))", booking);

        Assert.Equal("expired", await connection.ScalarAsync<string>("SELECT status FROM bookings WHERE id = $1", stale));
    }

    [Fact]
    public async Task Audit_log_excludes_ticket_token()
    {
        await using var connection = await database.OpenAsync();
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), await connection.CreateLocationAsync());

        Assert.False(await connection.ScalarAsync<bool>(
            "SELECT bool_or(metadata ? 'ticket_token') FROM audit_logs WHERE entity_type = 'bookings' AND entity_id = $1", booking));
    }
}
