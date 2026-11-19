using GjejVend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace GjejVend.Api.Authentication;

public static class AuthenticationSetup
{
    public static IServiceCollection AddSupabaseAuthentication(this IServiceCollection services, string supabaseUrl)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{supabaseUrl.TrimEnd('/')}/auth/v1";
                options.Audience = SupabaseClaims.Audience;
                options.MapInboundClaims = false;
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        return services;
    }
}
