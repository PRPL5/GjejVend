using GjejVend.Infrastructure.Persistence;

namespace GjejVend.Application.Common;

public static class CurrentUserExtensions
{
    public static Guid RequireId(this ICurrentUser currentUser) =>
        currentUser.Id ?? throw new InvalidOperationException("This operation requires a signed-in user.");
}
