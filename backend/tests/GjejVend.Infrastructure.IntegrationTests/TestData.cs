using Npgsql;

namespace GjejVend.Infrastructure.IntegrationTests;

internal static class TestData
{
    public const string OneHourFromNow = "now() + interval '1 hour'";

    public static async Task<Guid> CreateUserAsync(this NpgsqlConnection connection)
    {
        var id = Guid.NewGuid();
        await connection.ExecuteAsync("INSERT INTO auth.users (id, email) VALUES ($1, $2)", id, $"{id}@example.test");
        return id;
    }

    public static async Task<Guid> CreateLocationAsync(
        this NpgsqlConnection connection,
        int capacity = 1,
        string inventoryMode = "capacity",
        string opensAt = "00:00",
        string closesAt = "00:00",
        double latitude = 42.6629,
        double longitude = 21.1655)
    {
        var ownerId = await connection.CreateUserAsync();
        var providerId = await connection.ScalarAsync<Guid>(
            "INSERT INTO parking_providers (owner_id, display_name) VALUES ($1, 'Provider') RETURNING id", ownerId);
        var locationId = await connection.ScalarAsync<Guid>(
            """
            INSERT INTO parking_locations (provider_id, name, slug, address_line, city, latitude, longitude, total_capacity, inventory_mode, listing_status, parking_type)
            VALUES ($1, 'Lot', $2, 'Street 1', 'Prishtina', $3, $4, $5, $6, 'published', 'outdoor_lot')
            RETURNING id
            """,
            providerId, Guid.NewGuid().ToString(), (decimal)latitude, (decimal)longitude, capacity, inventoryMode);
        await connection.ExecuteAsync(
            "INSERT INTO parking_operating_hours (location_id, weekday, opens_at, closes_at) SELECT $1, d, $2::time, $3::time FROM generate_series(1, 7) d",
            locationId, opensAt, closesAt);
        await connection.ExecuteAsync(
            "INSERT INTO parking_rates (location_id, hourly_rate, daily_rate, valid_from) VALUES ($1, 1.50, 10.00, now() - interval '1 day')",
            locationId);
        return locationId;
    }

    public static Task<Guid> AddSpaceAsync(this NpgsqlConnection connection, Guid locationId) =>
        connection.ScalarAsync<Guid>(
            "INSERT INTO parking_spaces (location_id, label, vehicle_type) VALUES ($1, $2, 'car') RETURNING id", locationId, Guid.NewGuid().ToString());

    public static Task<Guid> BookAsync(this NpgsqlConnection connection, Guid customerId, Guid locationId, int startHours = 1, int endHours = 4, Guid? spaceId = null) =>
        connection.ScalarAsync<Guid>(
            "SELECT id FROM create_booking($1, $2, now() + make_interval(hours => $3), now() + make_interval(hours => $4), 'AB123CD', 'car', $5)",
            customerId, locationId, startHours, endHours, (object?)spaceId ?? DBNull.Value);

    public static string BookSql(int startHours, int endHours) =>
        $"SELECT create_booking($1, $2, now() + interval '{startHours} hours', now() + interval '{endHours} hours', 'AB123CD', 'car')";
}
