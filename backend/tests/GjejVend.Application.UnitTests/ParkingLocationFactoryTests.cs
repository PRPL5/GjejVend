using GjejVend.Application.Common;
using GjejVend.Application.Listings;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Listings;

namespace GjejVend.Application.UnitTests;

internal sealed class FakeListingRepository(params string[] takenSlugs) : IListingRepository
{
    public Task<ParkingLocation?> FindByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<ParkingLocation?>(null);
    public Task<IReadOnlyList<ParkingLocation>> ListByProviderAsync(Guid providerId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ParkingLocation>>([]);
    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) => Task.FromResult(takenSlugs.Contains(slug));
    public void Add(ParkingLocation location) { }
    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class ParkingLocationFactoryTests
{
    private static ListingRequest Request(
        string name = "Parking Qendra", decimal latitude = 42.6629m, decimal longitude = 21.1655m, int capacity = 40,
        string? city = null) =>
        new(name, null, "Rr. Agim Ramadani 12", "Qendra", city, null, latitude, longitude,
            ParkingType.Garage, InventoryMode.Capacity, capacity, null, "Kodi 1234", null);

    [Theory]
    [InlineData("Parking Qendra", "parking-qendra")]
    [InlineData("Parking Qendra – Sheshi Nëna Terezë", "parking-qendra-sheshi-nena-tereze")]
    [InlineData("  Garazha   Çarshia!! ", "garazha-carshia")]
    [InlineData("???", "parking")]
    public void Slugify_makes_a_clean_url_name(string name, string expected)
        => Assert.Equal(expected, ParkingLocationFactory.Slugify(name));

    [Fact]
    public async Task New_lot_is_a_draft_with_the_platform_defaults()
    {
        var provider = Guid.NewGuid();

        var lot = await new ParkingLocationFactory(new FakeListingRepository()).CreateAsync(provider, Request());

        Assert.Equal(provider, lot.ProviderId);
        Assert.Equal("parking-qendra", lot.Slug);
        Assert.Equal(ListingStatus.Draft, lot.ListingStatus);
        Assert.Equal(VerificationStatus.Unverified, lot.VerificationStatus);
        Assert.Equal(ParkingLocationFactory.DefaultCity, lot.City);
        Assert.Equal("XK", lot.CountryCode);
        Assert.Equal("Kodi 1234", lot.AccessInstructions);
    }

    [Fact]
    public async Task A_taken_slug_gets_the_next_free_number()
    {
        var factory = new ParkingLocationFactory(new FakeListingRepository("parking-qendra", "parking-qendra-2"));

        var lot = await factory.CreateAsync(Guid.NewGuid(), Request());

        Assert.Equal("parking-qendra-3", lot.Slug);
    }

    [Theory]
    [InlineData("", 42.6629, 21.1655, 40)]
    [InlineData("Lot", 42.6629, 21.1655, 0)]
    [InlineData("Lot", 41.3275, 19.8187, 40)]
    [InlineData("Lot", 42.6629, 21.5, 40)]
    public async Task Invalid_lots_are_rejected(string name, double latitude, double longitude, int capacity)
    {
        var factory = new ParkingLocationFactory(new FakeListingRepository());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            factory.CreateAsync(Guid.NewGuid(), Request(name, (decimal)latitude, (decimal)longitude, capacity)));
    }

    [Fact]
    public async Task Editing_keeps_the_slug_and_the_status()
    {
        var factory = new ParkingLocationFactory(new FakeListingRepository());
        var lot = await factory.CreateAsync(Guid.NewGuid(), Request());
        lot.ListingStatus = ListingStatus.Published;

        factory.Apply(lot, Request("A new name", capacity: 50, city: "Fushë Kosovë"));

        Assert.Equal("A new name", lot.Name);
        Assert.Equal(50, lot.TotalCapacity);
        Assert.Equal("Fushë Kosovë", lot.City);
        Assert.Equal("parking-qendra", lot.Slug);
        Assert.Equal(ListingStatus.Published, lot.ListingStatus);
    }
}
