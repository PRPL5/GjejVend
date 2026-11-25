using GjejVend.Application.Common;
using GjejVend.Application.Listings;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Listings;
using GjejVend.Infrastructure.Persistence;

namespace GjejVend.Infrastructure.IntegrationTests;

/// <summary>
/// M1 parking lots against a real database: the check constraints, the unique slug index and the soft delete
/// are all part of what is tested.
/// </summary>
[Collection(DatabaseGroup.Name)]
public sealed class ListingFlowTests(DatabaseFixture database)
{
    private sealed record TestUser(Guid? Id) : ICurrentUser;

    private static ListingService Listings(GjejVendDbContext db, Guid user)
    {
        var listings = new ListingRepository(db);
        return new ListingService(listings, new ProviderRepository(db), new ParkingLocationFactory(listings),
            new AccessGuard(new TestUser(user), db), TimeProvider.System);
    }

    private static ListingRequest Lot(string name = "Parking Qendra", int capacity = 40) =>
        new(name, null, "Rr. Agim Ramadani 12", "Qendra", null, null, 42.6629m, 21.1655m,
            ParkingType.Garage, InventoryMode.Capacity, capacity, "Hyrja nga rruga anësore.", "Kodi 1234", null);

    private async Task<Guid> OwnerAsync()
    {
        await using var connection = await database.OpenAsync();
        var user = await connection.CreateUserAsync();
        await using var db = database.CreateContext(new TestUser(user));
        await new ProviderService(new ProviderRepository(db), new AccessGuard(new TestUser(user), db))
            .CreateAsync(new ProviderRequest(ProviderType.Individual, "Owner", null, null, null, null, null));
        return user;
    }

    [Fact]
    public async Task A_new_lot_is_stored_as_a_draft_with_its_pin()
    {
        var owner = await OwnerAsync();
        await using var db = database.CreateContext(new TestUser(owner));

        var lot = await Listings(db, owner).CreateAsync(Lot());

        await using var connection = await database.OpenAsync();
        Assert.Equal("draft", await connection.ScalarAsync<string>("SELECT listing_status FROM parking_locations WHERE id = $1", lot.Id));
        Assert.Equal("garage", await connection.ScalarAsync<string>("SELECT parking_type FROM parking_locations WHERE id = $1", lot.Id));
        Assert.Equal(42.6629m, await connection.ScalarAsync<decimal>("SELECT latitude FROM parking_locations WHERE id = $1", lot.Id));
    }

    [Fact]
    public async Task Two_lots_with_the_same_name_get_different_slugs()
    {
        var owner = await OwnerAsync();
        await using var db = database.CreateContext(new TestUser(owner));
        var service = Listings(db, owner);
        var name = $"Garazha {Guid.NewGuid():N}";

        var first = await service.CreateAsync(Lot(name));
        var second = await service.CreateAsync(Lot(name));

        Assert.NotEqual(first.Slug, second.Slug);
        Assert.Equal($"{first.Slug}-2", second.Slug);
    }

    [Fact]
    public async Task Another_owner_cannot_edit_the_lot()
    {
        var owner = await OwnerAsync();
        var other = await OwnerAsync();
        Guid lotId;
        await using (var db = database.CreateContext(new TestUser(owner)))
            lotId = (await Listings(db, owner).CreateAsync(Lot())).Id;

        await using var otherDb = database.CreateContext(new TestUser(other));
        await Assert.ThrowsAsync<ForbiddenException>(() => Listings(otherDb, other).UpdateAsync(lotId, Lot("Taken over")));

        await using var connection = await database.OpenAsync();
        Assert.Equal("Parking Qendra", await connection.ScalarAsync<string>("SELECT name FROM parking_locations WHERE id = $1", lotId));
    }

    [Fact]
    public async Task Deleting_keeps_the_row_and_hides_the_lot()
    {
        var owner = await OwnerAsync();
        await using var db = database.CreateContext(new TestUser(owner));
        var service = Listings(db, owner);
        var lot = await service.CreateAsync(Lot());

        await service.DeleteAsync(lot.Id);

        await using var connection = await database.OpenAsync();
        Assert.True(await connection.ScalarAsync<bool>("SELECT deleted_at IS NOT NULL FROM parking_locations WHERE id = $1", lot.Id));
        Assert.Empty(await service.ListMineAsync());
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetMineAsync(lot.Id));
    }
}
