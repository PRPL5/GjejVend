using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GjejVend.Api.Controllers;
using GjejVend.Application.Listings;
using GjejVend.Domain.Identity;

namespace GjejVend.Api.IntegrationTests;

/// <summary>The provider endpoints over HTTP, with the in-memory store in place of the database.</summary>
public sealed class ProvidersApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private HttpClient ClientFor(Guid userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateToken(userId));
        return client;
    }

    [Fact]
    public async Task After_becoming_a_provider_me_shows_the_provider_role()
    {
        using var client = ClientFor(Guid.NewGuid());

        using var created = await client.PostAsJsonAsync("/api/providers", new { providerType = "Individual", displayName = "Parking Qendra" });
        var me = await client.GetFromJsonAsync<MeResponse>("/me", Json);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("/api/providers/me", created.Headers.Location?.AbsolutePath);
        Assert.Contains(Role.Provider, me!.Roles);
    }

    [Fact]
    public async Task Profile_is_not_found_before_becoming_a_provider()
    {
        using var client = ClientFor(Guid.NewGuid());

        using var response = await client.GetAsync(new Uri("/api/providers/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Creating_a_second_profile_is_a_bad_request()
    {
        using var client = ClientFor(Guid.NewGuid());
        var body = new { providerType = "Individual", displayName = "Parking Qendra" };
        (await client.PostAsJsonAsync("/api/providers", body)).Dispose();

        using var second = await client.PostAsJsonAsync("/api/providers", body);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Owner_can_edit_their_profile()
    {
        using var client = ClientFor(Guid.NewGuid());
        (await client.PostAsJsonAsync("/api/providers", new { providerType = "Individual", displayName = "Parking Qendra" })).Dispose();

        using var updated = await client.PutAsJsonAsync("/api/providers/me", new { providerType = "Individual", displayName = "Parking Qendra 2" });
        var profile = await updated.Content.ReadFromJsonAsync<ProviderResponse>(Json);

        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Parking Qendra 2", profile!.DisplayName);
    }

    [Fact]
    public async Task Providers_endpoint_needs_a_signed_in_user()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/providers", new { providerType = "Individual", displayName = "X" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
