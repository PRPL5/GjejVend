using System.Security.Claims;
using System.Text;
using GjejVend.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GjejVend.Api.Extensions;

public static class AuthExtensions
{
    public const string AdminPolicy = "Admin";
    public const string ProviderPolicy = "Provider";

    /// <summary>Validates Supabase-issued JWTs. Roles come from a "roles" claim (array) or "role".</summary>
    public static IServiceCollection AddApiAuth(this IServiceCollection services, IConfiguration config)
    {
        var secret = config["Jwt:Secret"] is { Length: > 0 } s ? s : "dev-only-secret-that-is-at-least-32-bytes-long!!";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = !string.IsNullOrEmpty(config["Jwt:Issuer"]),
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidateAudience = !string.IsNullOrEmpty(config["Jwt:Audience"]),
                    ValidAudience = config["Jwt:Audience"],
                    NameClaimType = "sub",
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AdminPolicy, p => p.RequireAuthenticatedUser().RequireAssertion(c => HasRole(c.User, Roles.Admin)))
            .AddPolicy(ProviderPolicy, p => p.RequireAuthenticatedUser()
                .RequireAssertion(c => HasRole(c.User, Roles.Provider) || HasRole(c.User, Roles.Admin)));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        return services;
    }

    public static bool HasRole(ClaimsPrincipal user, string role)
        => user.Claims.Any(c => (c.Type is "roles" or "role" or ClaimTypes.Role)
                                && string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase));
}

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? UserId
        => Guid.TryParse(Principal?.FindFirst("sub")?.Value ?? Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : null;

    public bool IsInRole(string role) => Principal is not null && AuthExtensions.HasRole(Principal, role);
}
