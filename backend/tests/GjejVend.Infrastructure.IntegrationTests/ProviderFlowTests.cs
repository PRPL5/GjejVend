using GjejVend.Application.Common;
using GjejVend.Application.Identity;
using GjejVend.Application.Listings;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Identity;
using GjejVend.Infrastructure.Listings;
using GjejVend.Infrastructure.Persistence;

namespace GjejVend.Infrastructure.IntegrationTests;

/// <summary>
/// The M1 provider profile against a real database (migrations, constraints and the signup trigger
/// that grants the customer role), so the profile and the provider role are checked where they are stored.
/// </summary>
[Collection(DatabaseGroup.Name)]
public sealed class ProviderFlowTests(DatabaseFixture database)
{
    private sealed record TestUser(Guid? Id) : ICurrentUser;

    private static (ProviderService Providers, RoleService Roles) Build(GjejVendDbContext db, Guid user)
    {
        var access = new AccessGuard(new TestUser(user), db);
        return (new ProviderService(new ProviderRepository(db), access), new RoleService(new UserRoleRepository(db), access));
    }

    private static ProviderRequest Request(
        string displayName = "Parking Qendra", ProviderType type = ProviderType.Individual, string? legalName = null) =>
        new(type, displayName, legalName, null, null, null, "+38344123456");

    [Fact]
    public async Task Becoming_a_provider_saves_the_profile_and_grants_the_provider_role()
    {
        await using var connection = await database.OpenAsync();
        var user = await connection.CreateUserAsync();
        await using var db = database.CreateContext(new TestUser(user));
        var (providers, roles) = Build(db, user);

        var created = await providers.CreateAsync(Request());

        Assert.Equal("Parking Qendra", created.DisplayName);
        Assert.Equal(VerificationStatus.Unverified, created.VerificationStatus);
        Assert.Equal(user, await connection.ScalarAsync<Guid>("SELECT owner_id FROM parking_providers WHERE id = $1", created.Id));

        var myRoles = await roles.GetMyRolesAsync();
        Assert.Contains(Role.Customer, myRoles);
        Assert.Contains(Role.Provider, myRoles);
    }

    [Fact]
    public async Task A_user_cannot_create_a_second_provider_profile()
    {
        await using var connection = await database.OpenAsync();
        var user = await connection.CreateUserAsync();
        await using var db = database.CreateContext(new TestUser(user));
        var (providers, _) = Build(db, user);
        await providers.CreateAsync(Request());

        await Assert.ThrowsAsync<BusinessRuleException>(() => providers.CreateAsync(Request("Another lot owner")));
        Assert.Equal(1, await connection.ScalarAsync<long>("SELECT count(*) FROM parking_providers WHERE owner_id = $1", user));
    }

    [Fact]
    public async Task Reading_the_profile_before_becoming_a_provider_is_not_found()
    {
        await using var connection = await database.OpenAsync();
        var user = await connection.CreateUserAsync();
        await using var db = database.CreateContext(new TestUser(user));
        var (providers, _) = Build(db, user);

        await Assert.ThrowsAsync<NotFoundException>(() => providers.GetMineAsync());
    }

    [Fact]
    public async Task Updating_the_profile_changes_the_stored_row()
    {
        await using var connection = await database.OpenAsync();
        var user = await connection.CreateUserAsync();
        await using var db = database.CreateContext(new TestUser(user));
        var (providers, _) = Build(db, user);
        var created = await providers.CreateAsync(Request());

        await providers.UpdateMineAsync(Request("Garazha Skënderbeu", ProviderType.Business, "Skënderbeu SH.P.K."));

        Assert.Equal("Garazha Skënderbeu", await connection.ScalarAsync<string>("SELECT display_name FROM parking_providers WHERE id = $1", created.Id));
        Assert.Equal("business", await connection.ScalarAsync<string>("SELECT provider_type FROM parking_providers WHERE id = $1", created.Id));
    }

    [Fact]
    public async Task A_business_needs_its_legal_name()
    {
        await using var connection = await database.OpenAsync();
        var user = await connection.CreateUserAsync();
        await using var db = database.CreateContext(new TestUser(user));
        var (providers, _) = Build(db, user);

        await Assert.ThrowsAsync<BusinessRuleException>(() => providers.CreateAsync(Request(type: ProviderType.Business)));
        Assert.Equal(0, await connection.ScalarAsync<long>("SELECT count(*) FROM parking_providers WHERE owner_id = $1", user));
    }
}
