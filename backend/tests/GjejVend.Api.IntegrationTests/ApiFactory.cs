using System.Security.Cryptography;
using GjejVend.Api.Workers;
using GjejVend.Infrastructure.Identity;
using GjejVend.Infrastructure.Listings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace GjejVend.Api.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private const string Issuer = "https://test-project.supabase.co/auth/v1";

    private readonly ECDsaSecurityKey _signingKey = new(ECDsa.Create(ECCurve.NamedCurves.nistP256));

    public InMemoryStore Store { get; } = new();

    public string CreateToken(Guid subject, string audience = "authenticated", DateTime? expires = null, SecurityKey? signingKey = null)
    {
        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(5);
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = audience,
            Claims = new Dictionary<string, object> { ["sub"] = subject.ToString(), ["role"] = "authenticated" },
            IssuedAt = expiresAt.AddMinutes(-15),
            NotBefore = expiresAt.AddMinutes(-15),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(signingKey ?? _signingKey, SecurityAlgorithms.EcdsaSha256),
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=unused;Username=unused;Password=unused");
        builder.UseSetting("Supabase:Url", "https://test-project.supabase.co");
        builder.UseSetting("Bookings:CloseOverdue", "false");
        builder.ConfigureTestServices(services =>
        {
            foreach (var worker in services.Where(d => d.ImplementationType == typeof(BookingLifecycleWorker)).ToList())
            {
                services.Remove(worker);
            }

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
                options.TokenValidationParameters.ValidIssuer = Issuer;
                options.TokenValidationParameters.IssuerSigningKey = _signingKey;
            });

            services.AddSingleton(Store);
            services.AddScoped<IUserRoleRepository>(_ => Store);
            services.AddScoped<IProviderRepository>(_ => Store);
            services.AddScoped<IListingRepository>(_ => Store);
            ConfigureServices(services);
        });
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _signingKey.ECDsa.Dispose();
        }

        base.Dispose(disposing);
    }
}
