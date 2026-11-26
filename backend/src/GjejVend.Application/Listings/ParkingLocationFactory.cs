using System.Globalization;
using System.Text;
using GjejVend.Application.Common;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Listings;

namespace GjejVend.Application.Listings;

/// <summary>What an owner sends to create or edit a parking lot. Status, slug and owner are set by the server.</summary>
public sealed record ListingRequest(
    string Name,
    string? Description,
    string AddressLine,
    string? Neighbourhood,
    string? City,
    string? PostalCode,
    decimal Latitude,
    decimal Longitude,
    ParkingType ParkingType,
    InventoryMode InventoryMode,
    int TotalCapacity,
    string? ArrivalInstructions,
    string? AccessInstructions,
    string? ContactPhone);

public interface IParkingLocationFactory
{
    /// <summary>Builds a new, valid draft lot for a provider, with a unique slug and the platform defaults.</summary>
    Task<ParkingLocation> CreateAsync(Guid providerId, ListingRequest request, CancellationToken ct = default);

    /// <summary>Validates an edit and copies the editable fields onto an existing lot. Slug and status do not change.</summary>
    void Apply(ParkingLocation location, ListingRequest request);
}

/// <summary>
/// Factory pattern (M1): the one place that knows how a valid ParkingLocation is built, so the service,
/// the wizard and the tests never assemble a lot by hand or forget a rule.
/// </summary>
public sealed class ParkingLocationFactory(IListingRepository listings) : IParkingLocationFactory
{
    public const string DefaultCity = "Prishtinë";

    // Pristina and its edges. A pin outside this box is almost always a mistake on the map.
    public const decimal MinLatitude = 42.55m;
    public const decimal MaxLatitude = 42.75m;
    public const decimal MinLongitude = 21.05m;
    public const decimal MaxLongitude = 21.30m;

    private const int MaxSlugLength = 60;

    public async Task<ParkingLocation> CreateAsync(Guid providerId, ListingRequest request, CancellationToken ct = default)
    {
        Validate(request);

        var location = new ParkingLocation
        {
            ProviderId = providerId,
            Name = request.Name.Trim(),
            Slug = await UniqueSlugAsync(request.Name, ct),
            AddressLine = request.AddressLine.Trim(),
            City = DefaultCity,
            ListingStatus = ListingStatus.Draft,
            VerificationStatus = VerificationStatus.Unverified,
            CountryCode = "XK",
            Timezone = "Europe/Belgrade",
        };
        Copy(location, request);
        return location;
    }

    public void Apply(ParkingLocation location, ListingRequest request)
    {
        Validate(request);
        location.Name = request.Name.Trim();
        location.AddressLine = request.AddressLine.Trim();
        Copy(location, request);
    }

    private static void Copy(ParkingLocation location, ListingRequest request)
    {
        location.Description = Clean(request.Description);
        location.Neighbourhood = Clean(request.Neighbourhood);
        location.City = Clean(request.City) ?? DefaultCity;
        location.PostalCode = Clean(request.PostalCode);
        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;
        location.ParkingType = request.ParkingType;
        location.InventoryMode = request.InventoryMode;
        location.TotalCapacity = request.TotalCapacity;
        location.ArrivalInstructions = Clean(request.ArrivalInstructions);
        location.AccessInstructions = Clean(request.AccessInstructions);
        location.ContactPhone = Clean(request.ContactPhone);
    }

    private static void Validate(ListingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BusinessRuleException("The lot needs a name.");
        if (string.IsNullOrWhiteSpace(request.AddressLine))
            throw new BusinessRuleException("The lot needs an address.");
        if (request.TotalCapacity <= 0)
            throw new BusinessRuleException("Capacity must be at least 1 spot.");
        if (request.Latitude is < MinLatitude or > MaxLatitude || request.Longitude is < MinLongitude or > MaxLongitude)
            throw new BusinessRuleException("The pin must be in Pristina.");
    }

    private async Task<string> UniqueSlugAsync(string name, CancellationToken ct)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        for (var n = 2; await listings.SlugExistsAsync(slug, ct); n++)
            slug = $"{baseSlug}-{n}";
        return slug;
    }

    /// <summary>"Parking Qendra – Sheshi Nëna Terezë" becomes "parking-qendra-sheshi-nena-tereze".</summary>
    public static string Slugify(string text)
    {
        var plain = new StringBuilder();
        foreach (var c in text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            plain.Append(char.IsAsciiLetterOrDigit(c) ? c : '-');
        }

        var slug = string.Join('-', plain.ToString().Split('-', StringSplitOptions.RemoveEmptyEntries));
        if (slug.Length > MaxSlugLength) slug = slug[..MaxSlugLength].TrimEnd('-');
        return slug.Length == 0 ? "parking" : slug;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
