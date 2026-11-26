using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GjejVend.Application.Listings;
using GjejVend.Domain.Listings;

namespace GjejVend.Api.IntegrationTests;

/// <summary>The parking lot endpoints over HTTP, with the in-memory store in place of the database.</summary>
public sealed class ListingsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private static object Lot(string name = "Parking Qendra", decimal latitude = 42.6629m, decimal longitude = 21.1655m, int capacity = 40) => new
    {
        name,
        addressLine = "Rr. Agim Ramadani 12",
        neighbourhood = "Qendra",
        latitude,
        longitude,
        parkingType = "Garage",
        inventoryMode = "Capacity",
        totalCapacity = capacity,
        arrivalInstructions = "Hyrja nga rruga anësore.",
        accessInstructions = "Kodi i portës 1234.",
    };

    private HttpClient ClientFor(Guid userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));
        return client;
    }

    private async Task<HttpClient> OwnerAsync()
    {
        var client = ClientFor(Guid.NewGuid());
        (await client.PostAsJsonAsync("/api/providers", new { providerType = "Individual", displayName = "Owner" })).Dispose();
        return client;
    }

    private static async Task<ListingResponse> ReadLotAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ListingResponse>(Json))!;

    [Fact]
    public async Task Owner_creates_a_draft_lot_with_a_pin_and_edits_it()
    {
        using var owner = await OwnerAsync();

        using var created = await owner.PostAsJsonAsync("/api/listings", Lot());
        var lot = await ReadLotAsync(created);
        using var edited = await owner.PutAsJsonAsync($"/api/listings/{lot.Id}", Lot("Parking Qendra 24/7", capacity: 45));
        var changed = await ReadLotAsync(edited);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(ListingStatus.Draft, lot.ListingStatus);
        Assert.Equal("parking-qendra", lot.Slug);
        Assert.Equal(42.6629m, lot.Latitude);
        Assert.Equal(HttpStatusCode.OK, edited.StatusCode);
        Assert.Equal("Parking Qendra 24/7", changed.Name);
        Assert.Equal(45, changed.TotalCapacity);
        Assert.Equal("parking-qendra", changed.Slug);
    }

    [Fact]
    public async Task Another_owner_gets_forbidden()
    {
        using var owner = await OwnerAsync();
        using var other = await OwnerAsync();
        var lot = await ReadLotAsync(await owner.PostAsJsonAsync("/api/listings", Lot()));

        using var read = await other.GetAsync(new Uri($"/api/listings/{lot.Id}", UriKind.Relative));
        using var edit = await other.PutAsJsonAsync($"/api/listings/{lot.Id}", Lot("Taken over"));
        using var delete = await other.DeleteAsync(new Uri($"/api/listings/{lot.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Forbidden, read.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, edit.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task A_user_without_a_provider_profile_cannot_list_a_lot()
    {
        using var client = ClientFor(Guid.NewGuid());

        using var response = await client.PostAsJsonAsync("/api/listings", Lot());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_pin_outside_pristina_is_a_bad_request()
    {
        using var owner = await OwnerAsync();

        using var response = await owner.PostAsJsonAsync("/api/listings", Lot(latitude: 41.3275m, longitude: 19.8187m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Mine_lists_only_my_lots_and_a_deleted_lot_disappears()
    {
        using var owner = await OwnerAsync();
        using var other = await OwnerAsync();
        var first = await ReadLotAsync(await owner.PostAsJsonAsync("/api/listings", Lot("Lot A")));
        (await owner.PostAsJsonAsync("/api/listings", Lot("Lot B"))).Dispose();
        (await other.PostAsJsonAsync("/api/listings", Lot("Someone else"))).Dispose();

        using var deleted = await owner.DeleteAsync(new Uri($"/api/listings/{first.Id}", UriKind.Relative));
        var mine = await owner.GetFromJsonAsync<List<ListingResponse>>("/api/listings/mine", Json);
        using var gone = await owner.GetAsync(new Uri($"/api/listings/{first.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(["Lot B"], mine!.Select(l => l.Name));
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }
}
