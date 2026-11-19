namespace GjejVend.Infrastructure.IntegrationTests;

[Collection(DatabaseGroup.Name)]
public sealed class IdentityTests(DatabaseFixture database)
{
    [Fact]
    public async Task Signup_creates_profile_with_customer_role()
    {
        await using var connection = await database.OpenAsync();
        var id = Guid.NewGuid();

        await connection.ExecuteAsync(
            "INSERT INTO auth.users (id, phone, raw_user_meta_data) VALUES ($1, '38344111222', '{\"first_name\":\"Drin\"}')", id);

        Assert.Equal("Drin", await connection.ScalarAsync<string>("SELECT first_name FROM profiles WHERE id = $1", id));
        Assert.Equal("customer", await connection.ScalarAsync<string>("SELECT string_agg(role, ',') FROM user_roles WHERE user_id = $1", id));
    }

    [Fact]
    public async Task Auth_contact_change_syncs_to_profile()
    {
        await using var connection = await database.OpenAsync();
        var id = await connection.CreateUserAsync();

        await connection.ExecuteAsync("UPDATE auth.users SET email = 'new@example.test' WHERE id = $1", id);

        Assert.Equal("new@example.test", await connection.ScalarAsync<string>("SELECT email FROM profiles WHERE id = $1", id));
    }
}
