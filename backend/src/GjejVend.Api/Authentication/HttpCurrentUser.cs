using GjejVend.Infrastructure.Persistence;

namespace GjejVend.Api.Authentication;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? Id =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirst(SupabaseClaims.Subject)?.Value, out var id) ? id : null;
}
