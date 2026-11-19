using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using GjejVend.Api.Controllers;
using Microsoft.IdentityModel.Tokens;

namespace GjejVend.Api.IntegrationTests;

public sealed class AuthenticationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Me_without_token_returns_unauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_with_valid_token_returns_subject()
    {
        var userId = Guid.NewGuid();

        using var response = await GetMeAsync(factory.CreateToken(userId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(userId, (await response.Content.ReadFromJsonAsync<MeResponse>())!.Id);
    }

    [Fact]
    public async Task Token_for_another_audience_is_rejected()
    {
        using var response = await GetMeAsync(factory.CreateToken(Guid.NewGuid(), audience: "anon"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Expired_token_is_rejected()
    {
        using var response = await GetMeAsync(factory.CreateToken(Guid.NewGuid(), expires: DateTime.UtcNow.AddMinutes(-10)));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_signed_with_unknown_key_is_rejected()
    {
        using var foreignKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        using var response = await GetMeAsync(factory.CreateToken(Guid.NewGuid(), signingKey: new ECDsaSecurityKey(foreignKey)));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<HttpResponseMessage> GetMeAsync(string token)
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.GetAsync(new Uri("/me", UriKind.Relative));
    }
}
