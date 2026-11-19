namespace GjejVend.Infrastructure.IntegrationTests;

[Collection(DatabaseGroup.Name)]
public sealed class SearchTests(DatabaseFixture database)
{
    [Fact]
    public async Task Search_returns_nearby_locations_by_distance_with_availability()
    {
        await using var connection = await database.OpenAsync();
        var near = await connection.CreateLocationAsync(capacity: 2, latitude: 41.0001, longitude: 20.0001);
        var nearer = await connection.CreateLocationAsync(capacity: 1, latitude: 41.0000, longitude: 20.0000);
        var far = await connection.CreateLocationAsync(latitude: 41.2000, longitude: 20.2000);
        await connection.BookAsync(await connection.CreateUserAsync(), nearer, 1, 3);

        var results = await connection.ScalarAsync<string>(
            """
            SELECT string_agg(location_id || ':' || free_capacity, ',' ORDER BY distance_m)
            FROM search_parking(41.0, 20.0, 1000, now() + interval '2 hours', now() + interval '3 hours')
            """);

        Assert.Equal($"{nearer}:0,{near}:2", results);
        Assert.DoesNotContain(far.ToString(), results, StringComparison.Ordinal);
    }
}
