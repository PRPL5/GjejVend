using Npgsql;

namespace GjejVend.Infrastructure.IntegrationTests;

[Collection(DatabaseGroup.Name)]
public sealed class PaymentTests(DatabaseFixture database)
{
    [Fact]
    public async Task Successful_payment_writes_ledger_entries()
    {
        await using var connection = await database.OpenAsync();
        var payment = await CreatePaidBookingAsync(connection);

        var entries = await connection.ScalarAsync<string>(
            "SELECT string_agg(entry_type || '=' || amount, ',' ORDER BY entry_type) FROM ledger_entries WHERE payment_id = $1", payment);

        Assert.Equal("charge=4.50,commission=0.45,provider_earning=4.05", entries);
    }

    [Fact]
    public async Task Refund_cannot_exceed_payment()
    {
        await using var connection = await database.OpenAsync();
        var payment = await CreatePaidBookingAsync(connection);

        var error = await connection.FailureAsync("INSERT INTO refunds (payment_id, amount) VALUES ($1, 5.00)", payment);

        Assert.Equal("refund_exceeds_payment", error);
    }

    [Fact]
    public async Task Successful_refund_updates_payment_status_and_ledger()
    {
        await using var connection = await database.OpenAsync();
        var payment = await CreatePaidBookingAsync(connection);

        await connection.ExecuteAsync("INSERT INTO refunds (payment_id, amount, status) VALUES ($1, 1.00, 'succeeded')", payment);

        Assert.Equal("partially_refunded", await connection.ScalarAsync<string>("SELECT status FROM payments WHERE id = $1", payment));
        Assert.Equal(1.00m, await connection.ScalarAsync<decimal>("SELECT amount FROM ledger_entries WHERE payment_id = $1 AND entry_type = 'refund'", payment));
    }

    [Fact]
    public async Task Financial_records_are_protected()
    {
        await using var connection = await database.OpenAsync();
        var payment = await CreatePaidBookingAsync(connection);

        Assert.Equal("DELETE on payments is not allowed", await connection.FailureAsync("DELETE FROM payments WHERE id = $1", payment));
        Assert.Equal("UPDATE on ledger_entries is not allowed", await connection.FailureAsync("UPDATE ledger_entries SET amount = 0 WHERE payment_id = $1", payment));
        Assert.Equal("DELETE on audit_logs is not allowed", await connection.FailureAsync("DELETE FROM audit_logs WHERE entity_id = $1", payment));
    }

    [Fact]
    public async Task Ledger_entry_type_is_required()
    {
        await using var connection = await database.OpenAsync();

        var exception = await Assert.ThrowsAsync<PostgresException>(() => connection.ExecuteAsync("INSERT INTO ledger_entries (amount) VALUES (1)"));

        Assert.Equal("23502", exception.SqlState);
    }

    [Fact]
    public async Task Payment_changes_are_audited_with_actor()
    {
        await using var connection = await database.OpenAsync();
        var actor = await connection.CreateUserAsync();
        await connection.ExecuteAsync($"SET app.actor_id = '{actor}'");

        var payment = await CreatePaidBookingAsync(connection);
        await connection.ExecuteAsync("RESET app.actor_id");

        Assert.Equal(actor, await connection.ScalarAsync<Guid>("SELECT actor_id FROM audit_logs WHERE entity_type = 'payments' AND entity_id = $1", payment));
    }

    private static async Task<Guid> CreatePaidBookingAsync(NpgsqlConnection connection)
    {
        var booking = await connection.BookAsync(await connection.CreateUserAsync(), await connection.CreateLocationAsync());
        return await connection.ScalarAsync<Guid>(
            "INSERT INTO payments (booking_id, customer_id, amount, provider, status) SELECT id, customer_id, total_amount, 'test', 'succeeded' FROM bookings WHERE id = $1 RETURNING id",
            booking);
    }
}
