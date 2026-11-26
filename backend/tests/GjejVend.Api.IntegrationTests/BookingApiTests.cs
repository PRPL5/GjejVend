using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GjejVend.Application.Bookings;
using GjejVend.Infrastructure.IntegrationTests;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Api.IntegrationTests;

public sealed class BookingApiTests(DatabaseApiFactory factory) : IClassFixture<DatabaseApiFactory>
{
    [Fact]
    public async Task Requests_without_token_are_rejected()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/bookings", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Preview_returns_price_free_spots_and_default_vehicle()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 3);
        var customer = await connection.CreateUserAsync();
        await connection.ExecuteAsync("INSERT INTO vehicles (user_id, plate, vehicle_type, is_default) VALUES ($1, '01XYZ999', 'suv', true)", customer);
        using var client = factory.ClientFor(customer);
        var start = DateTimeOffset.UtcNow.AddHours(1);

        var preview = await client.GetFromJsonAsync<JsonElement>(
            $"/bookings/preview?locationId={location}&startsAt={Uri.EscapeDataString(start.ToString("O"))}&endsAt={Uri.EscapeDataString(start.AddHours(3).ToString("O"))}");

        Assert.Equal(4.50m, preview.GetProperty("totalAmount").GetDecimal());
        Assert.Equal(3, preview.GetProperty("freeCapacity").GetInt32());
        Assert.True(preview.GetProperty("isOpen").GetBoolean());
        Assert.Equal("01XYZ999", preview.GetProperty("defaultVehicle").GetProperty("plate").GetString());
        Assert.Equal("Suv", preview.GetProperty("defaultVehicle").GetProperty("vehicleType").GetString());
    }

    [Fact]
    public async Task Two_parallel_requests_for_the_last_spot_give_one_success_and_one_conflict()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 1);
        using var first = factory.ClientFor(await connection.CreateUserAsync());
        using var second = factory.ClientFor(await connection.CreateUserAsync());

        var responses = await Task.WhenAll(
            first.PostAsJsonAsync("/bookings", Booking(location)),
            second.PostAsJsonAsync("/bookings", Booking(location)));

        Assert.Equal(
            [HttpStatusCode.Created, HttpStatusCode.Conflict],
            responses.Select(r => r.StatusCode).OrderBy(s => (int)s));
        var conflict = responses.Single(r => r.StatusCode == HttpStatusCode.Conflict);
        Assert.Equal("booking_no_capacity", await CodeAsync(conflict));
        Assert.All(responses, r => r.Dispose());
    }

    [Fact]
    public async Task Booking_outside_opening_hours_returns_conflict()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync(opensAt: "08:00", closesAt: "18:00");
        var startsAt = await connection.ScalarAsync<DateTime>("SELECT (current_date + 2 + time '03:00') AT TIME ZONE 'Europe/Belgrade'");
        using var client = factory.ClientFor(await connection.CreateUserAsync());

        using var response = await client.PostAsJsonAsync("/bookings", Booking(location, new DateTimeOffset(startsAt, TimeSpan.Zero), 2));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("booking_outside_opening_hours", await CodeAsync(response));
    }

    [Fact]
    public async Task Invalid_request_returns_field_errors()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync();
        using var client = factory.ClientFor(await connection.CreateUserAsync());

        using var response = await client.PostAsJsonAsync("/bookings", Booking(location, plate: "<x>"));
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(problem.GetProperty("errors").TryGetProperty("VehiclePlate", out _));
    }

    [Fact]
    public async Task Ticket_is_private_and_reveals_access_details_only_once_confirmed()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync();
        await connection.ExecuteAsync("UPDATE parking_locations SET access_instructions = 'Gate code 4321' WHERE id = $1", location);
        var customer = await connection.CreateUserAsync();
        using var client = factory.ClientFor(customer);
        using var stranger = factory.ClientFor(await connection.CreateUserAsync());
        var booking = await CreateAsync(client, location);

        var pending = await client.GetFromJsonAsync<JsonElement>($"/bookings/{booking}");
        await ConfirmAsync(booking);
        var confirmed = await client.GetFromJsonAsync<JsonElement>($"/bookings/{booking}");
        using var foreign = await stranger.GetAsync(new Uri($"/bookings/{booking}", UriKind.Relative));

        Assert.Equal("Pending", pending.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, pending.GetProperty("accessInstructions").ValueKind);
        Assert.Equal(JsonValueKind.Null, pending.GetProperty("ticketToken").ValueKind);
        Assert.Equal("Confirmed", confirmed.GetProperty("status").GetString());
        Assert.Equal("Gate code 4321", confirmed.GetProperty("accessInstructions").GetString());
        Assert.False(string.IsNullOrEmpty(confirmed.GetProperty("ticketToken").GetString()));
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
    }

    [Fact]
    public async Task Pending_booking_past_its_payment_window_cannot_be_confirmed()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync();
        using var client = factory.ClientFor(await connection.CreateUserAsync());
        var booking = await CreateAsync(client, location);
        await connection.ExecuteAsync("UPDATE bookings SET expires_at = now() - interval '1 minute' WHERE id = $1", booking);

        var exception = await Assert.ThrowsAsync<Domain.Common.ConflictException>(() => ConfirmAsync(booking));

        Assert.Equal("booking_expired", exception.Code);
        Assert.Equal("pending", await connection.ScalarAsync<string>("SELECT status FROM bookings WHERE id = $1", booking));
    }

    [Fact]
    public async Task Extending_into_a_full_period_conflicts_and_a_valid_extension_updates_the_ticket()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 1);
        using var client = factory.ClientFor(await connection.CreateUserAsync());
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var booking = await CreateAsync(client, location, start, 1);
        await ConfirmAsync(booking);
        await connection.BookAsync(await connection.CreateUserAsync(), location, 3, 4);

        using var valid = await client.PostAsJsonAsync($"/bookings/{booking}/extend", new { endsAt = start.AddHours(2) });
        using var full = await client.PostAsJsonAsync($"/bookings/{booking}/extend", new { endsAt = start.AddHours(3) });
        var ticket = await valid.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
        Assert.Equal(3.00m, ticket.GetProperty("totalAmount").GetDecimal());
        Assert.Equal(start.AddHours(2).UtcDateTime, ticket.GetProperty("endsAt").GetDateTime(), TimeSpan.FromSeconds(1));
        Assert.Equal(HttpStatusCode.Conflict, full.StatusCode);
        Assert.Equal("booking_no_capacity", await CodeAsync(full));
    }

    [Fact]
    public async Task Cancelling_notifies_customer_and_owner_and_cannot_repeat()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync();
        var customer = await connection.CreateUserAsync();
        var owner = await OwnerOfAsync(connection, location);
        using var client = factory.ClientFor(customer);
        var booking = await CreateAsync(client, location);
        await ConfirmAsync(booking);

        using var cancel = await client.PostAsJsonAsync($"/bookings/{booking}/cancel", new { reason = "Plans changed" });
        using var again = await client.PostAsJsonAsync($"/bookings/{booking}/cancel", new { reason = (string?)null });
        var ticket = await client.GetFromJsonAsync<JsonElement>($"/bookings/{booking}");

        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);
        Assert.Equal("Cancelled", ticket.GetProperty("status").GetString());
        Assert.Equal("Plans changed", ticket.GetProperty("cancellationReason").GetString());
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal(customer, await connection.ScalarAsync<Guid>("SELECT cancelled_by FROM bookings WHERE id = $1", booking));
        foreach (var user in new[] { customer, owner })
        {
            Assert.Equal(1L, await connection.ScalarAsync<long>(
                "SELECT count(*) FROM notifications WHERE user_id = $1 AND entity_id = $2 AND type = 'booking_cancelled'", user, booking));
        }
    }

    [Fact]
    public async Task Owner_scans_ticket_to_check_in_and_out_and_other_lots_are_rejected()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync();
        var otherLocation = await connection.CreateLocationAsync();
        using var customer = factory.ClientFor(await connection.CreateUserAsync());
        using var owner = factory.ClientFor(await OwnerOfAsync(connection, location));
        using var otherOwner = factory.ClientFor(await OwnerOfAsync(connection, otherLocation));
        var booking = await CreateAsync(customer, location);
        await ConfirmAsync(booking);
        var token = (await customer.GetFromJsonAsync<JsonElement>($"/bookings/{booking}")).GetProperty("ticketToken").GetString();

        using var wrongLot = await otherOwner.PostAsJsonAsync("/bookings/check-in", new { locationId = otherLocation, token });
        using var notOwner = await otherOwner.PostAsJsonAsync("/bookings/check-in", new { locationId = location, token });
        using var checkIn = await owner.PostAsJsonAsync("/bookings/check-in", new { locationId = location, token });
        using var checkOut = await owner.PostAsJsonAsync("/bookings/check-out", new { locationId = location, token });

        Assert.Equal(HttpStatusCode.Conflict, wrongLot.StatusCode);
        Assert.Equal("ticket_wrong_location", await CodeAsync(wrongLot));
        Assert.Equal(HttpStatusCode.Forbidden, notOwner.StatusCode);
        Assert.Equal("Active", (await checkIn.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("status").GetString());
        Assert.Equal("Completed", (await checkOut.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("status").GetString());
        Assert.True(await connection.ScalarAsync<bool>("SELECT checked_in_at IS NOT NULL AND checked_out_at IS NOT NULL FROM bookings WHERE id = $1", booking));
    }

    [Fact]
    public async Task Owner_sees_arrivals_without_ticket_tokens_and_checks_in_manually()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync();
        using var customer = factory.ClientFor(await connection.CreateUserAsync());
        using var owner = factory.ClientFor(await OwnerOfAsync(connection, location));
        var booking = await CreateAsync(customer, location);
        await ConfirmAsync(booking);
        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"));
        var to = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(1).ToString("O"));

        var arrivals = await owner.GetFromJsonAsync<JsonElement>($"/owner/arrivals?from={from}&to={to}");
        using var checkIn = await owner.PostAsync(new Uri($"/bookings/{booking}/check-in", UriKind.Relative), null);
        using var customerCheckIn = await customer.PostAsync(new Uri($"/bookings/{booking}/check-in", UriKind.Relative), null);

        var arrival = Assert.Single(arrivals.EnumerateArray());
        Assert.Equal("01ABC123", arrival.GetProperty("vehiclePlate").GetString());
        Assert.False(arrival.TryGetProperty("ticketToken", out _));
        Assert.Equal(HttpStatusCode.OK, checkIn.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, customerCheckIn.StatusCode);
    }

    [Fact]
    public async Task Completed_booking_can_be_reviewed_once_and_owner_can_reply()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync();
        using var customer = factory.ClientFor(await connection.CreateUserAsync());
        using var owner = factory.ClientFor(await OwnerOfAsync(connection, location));
        using var stranger = factory.ClientFor(await connection.CreateUserAsync());
        var booking = await CreateAsync(customer, location);
        await ConfirmAsync(booking);

        using var early = await customer.PostAsJsonAsync($"/bookings/{booking}/review", new { rating = 5 });
        (await owner.PostAsync(new Uri($"/bookings/{booking}/check-in", UriKind.Relative), null)).Dispose();
        (await owner.PostAsync(new Uri($"/bookings/{booking}/check-out", UriKind.Relative), null)).Dispose();
        using var review = await customer.PostAsJsonAsync($"/bookings/{booking}/review", new { rating = 4, comment = "Easy to find" });
        using var duplicate = await customer.PostAsJsonAsync($"/bookings/{booking}/review", new { rating = 1 });
        var reviewId = (await review.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var foreignReply = await stranger.PostAsJsonAsync($"/reviews/{reviewId}/response", new { response = "Hi" });
        using var reply = await owner.PostAsJsonAsync($"/reviews/{reviewId}/response", new { response = "Thank you!" });
        var published = await factory.CreateClient().GetFromJsonAsync<JsonElement>($"/reviews?locationId={location}");
        var ownerReviews = await owner.GetFromJsonAsync<JsonElement>("/owner/reviews");

        Assert.Equal("review_booking_not_eligible", await CodeAsync(early));
        Assert.Equal(HttpStatusCode.Created, review.StatusCode);
        Assert.Equal("review_already_exists", await CodeAsync(duplicate));
        Assert.Equal(HttpStatusCode.Forbidden, foreignReply.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reply.StatusCode);
        Assert.Equal("Thank you!", Assert.Single(published.EnumerateArray()).GetProperty("providerResponse").GetString());
        Assert.Single(ownerReviews.EnumerateArray());
        Assert.Equal(4.00m, await connection.ScalarAsync<decimal>("SELECT rating_average FROM parking_locations WHERE id = $1", location));
        Assert.True((await customer.GetFromJsonAsync<JsonElement>($"/bookings/{booking}")).GetProperty("hasReview").GetBoolean());
    }

    [Fact]
    public async Task Overdue_bookings_close_without_manual_action()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync(capacity: 2);
        var customer = await connection.CreateUserAsync();
        using var client = factory.ClientFor(customer);
        var unattended = await CreateAsync(client, location);
        var parked = await CreateAsync(client, location);
        await ConfirmAsync(unattended);
        await ConfirmAsync(parked);
        await connection.ExecuteAsync("UPDATE bookings SET status = 'active' WHERE id = $1", parked);
        await connection.ExecuteAsync(
            "UPDATE bookings SET starts_at = now() - interval '3 hours', ends_at = now() - interval '1 hour' WHERE id = ANY($1)",
            new[] { unattended, parked });

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<BookingLifecycleService>().CloseOverdueAsync(CancellationToken.None);
        }

        Assert.Equal("no_show", await connection.ScalarAsync<string>("SELECT status FROM bookings WHERE id = $1", unattended));
        Assert.Equal("completed", await connection.ScalarAsync<string>("SELECT status FROM bookings WHERE id = $1", parked));
        Assert.Equal(1L, await connection.ScalarAsync<long>(
            "SELECT count(*) FROM notifications WHERE user_id = $1 AND entity_id = $2 AND type = 'review_reminder'", customer, parked));
    }

    [Fact]
    public async Task Favorites_can_be_saved_listed_and_removed()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync();
        using var client = factory.ClientFor(await connection.CreateUserAsync());

        using var save = await client.PutAsync(new Uri($"/me/favorites/{location}", UriKind.Relative), null);
        using var saveAgain = await client.PutAsync(new Uri($"/me/favorites/{location}", UriKind.Relative), null);
        using var missing = await client.PutAsync(new Uri($"/me/favorites/{Guid.NewGuid()}", UriKind.Relative), null);
        var saved = await client.GetFromJsonAsync<JsonElement>("/me/favorites");
        using var remove = await client.DeleteAsync(new Uri($"/me/favorites/{location}", UriKind.Relative));
        var afterRemove = await client.GetFromJsonAsync<JsonElement>("/me/favorites");

        Assert.Equal(HttpStatusCode.NoContent, save.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, saveAgain.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(location, Assert.Single(saved.EnumerateArray()).GetProperty("locationId").GetGuid());
        Assert.Equal(HttpStatusCode.NoContent, remove.StatusCode);
        Assert.Empty(afterRemove.EnumerateArray());
    }

    [Fact]
    public async Task Notifications_can_be_listed_and_marked_read()
    {
        await using var connection = await factory.OpenAsync();
        var location = await connection.CreateLocationAsync();
        using var client = factory.ClientFor(await connection.CreateUserAsync());
        var booking = await CreateAsync(client, location);
        await ConfirmAsync(booking);
        (await client.PostAsJsonAsync($"/bookings/{booking}/cancel", new { reason = (string?)null })).Dispose();

        var before = await client.GetFromJsonAsync<JsonElement>("/me/notifications");
        var first = before.GetProperty("items")[0].GetProperty("id").GetGuid();
        using var read = await client.PostAsync(new Uri($"/me/notifications/{first}/read", UriKind.Relative), null);
        using var readTwice = await client.PostAsync(new Uri($"/me/notifications/{first}/read", UriKind.Relative), null);
        using var readAll = await client.PostAsync(new Uri("/me/notifications/read-all", UriKind.Relative), null);
        var after = await client.GetFromJsonAsync<JsonElement>("/me/notifications");

        Assert.Equal(2, before.GetProperty("unreadCount").GetInt32());
        Assert.Equal(HttpStatusCode.NoContent, read.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, readTwice.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, readAll.StatusCode);
        Assert.Equal(0, after.GetProperty("unreadCount").GetInt32());
    }

    private static object Booking(Guid locationId, DateTimeOffset? startsAt = null, int hours = 3, string plate = "01abc123")
    {
        var start = startsAt ?? DateTimeOffset.UtcNow.AddHours(1);
        return new { locationId, startsAt = start, endsAt = start.AddHours(hours), vehiclePlate = plate, vehicleType = "car" };
    }

    private static async Task<Guid> CreateAsync(HttpClient client, Guid locationId, DateTimeOffset? startsAt = null, int hours = 3)
    {
        using var response = await client.PostAsJsonAsync("/bookings", Booking(locationId, startsAt, hours));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private async Task ConfirmAsync(Guid bookingId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IBookingConfirmation>().ConfirmAsync(bookingId, CancellationToken.None);
    }

    private static Task<Guid> OwnerOfAsync(Npgsql.NpgsqlConnection connection, Guid locationId) =>
        connection.ScalarAsync<Guid>(
            "SELECT p.owner_id FROM parking_locations l JOIN parking_providers p ON p.id = l.provider_id WHERE l.id = $1", locationId);

    private static async Task<string?> CodeAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();
}
