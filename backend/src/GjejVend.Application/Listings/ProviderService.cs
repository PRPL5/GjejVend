using GjejVend.Application.Common;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Listings;

namespace GjejVend.Application.Listings;

/// <summary>What an owner sends to create or edit their provider profile. Ownership and verification are set by the server.</summary>
public sealed record ProviderRequest(
    ProviderType ProviderType,
    string DisplayName,
    string? LegalName,
    string? TaxNumber,
    string? Description,
    string? ContactEmail,
    string? ContactPhone);

public sealed record ProviderResponse(
    Guid Id,
    ProviderType ProviderType,
    string DisplayName,
    string? LegalName,
    string? TaxNumber,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    VerificationStatus VerificationStatus,
    DateTime CreatedAt);

public interface IProviderService
{
    /// <summary>Makes the signed-in user a parking provider: creates their profile and grants the provider role.</summary>
    Task<ProviderResponse> CreateAsync(ProviderRequest request, CancellationToken ct = default);

    /// <summary>The signed-in user's provider profile. Throws NotFound when they are not a provider yet.</summary>
    Task<ProviderResponse> GetMineAsync(CancellationToken ct = default);

    /// <summary>Edits the signed-in user's provider profile.</summary>
    Task<ProviderResponse> UpdateMineAsync(ProviderRequest request, CancellationToken ct = default);
}

/// <summary>
/// Provider profiles (M1). One profile per user for now; the provider role is granted in the same save,
/// so the profile and the role are never out of step.
/// </summary>
public sealed class ProviderService(IProviderRepository providers, AccessGuard access) : IProviderService
{
    public async Task<ProviderResponse> CreateAsync(ProviderRequest request, CancellationToken ct = default)
    {
        var userId = access.RequireUser();
        Validate(request);

        if (await providers.ExistsForOwnerAsync(userId, ct))
            throw new BusinessRuleException("You already have a provider profile.");

        var provider = new ParkingProvider { OwnerId = userId, DisplayName = request.DisplayName.Trim() };
        Apply(provider, request);
        providers.Add(provider);

        if (!await providers.HasRoleAsync(userId, Role.Provider, ct))
            providers.AddRole(new UserRole { UserId = userId, Role = Role.Provider, GrantedBy = userId });

        await providers.SaveChangesAsync(ct);
        return ToResponse(provider);
    }

    public async Task<ProviderResponse> GetMineAsync(CancellationToken ct = default)
        => ToResponse(await FindMineAsync(ct));

    public async Task<ProviderResponse> UpdateMineAsync(ProviderRequest request, CancellationToken ct = default)
    {
        Validate(request);
        var provider = await FindMineAsync(ct);
        provider.DisplayName = request.DisplayName.Trim();
        Apply(provider, request);
        await providers.SaveChangesAsync(ct);
        return ToResponse(provider);
    }

    private async Task<ParkingProvider> FindMineAsync(CancellationToken ct)
    {
        var userId = access.RequireUser();
        return await providers.FindByOwnerAsync(userId, ct)
            ?? throw new NotFoundException("You don't have a provider profile yet.");
    }

    private static void Validate(ProviderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new BusinessRuleException("Display name is required.");
        if (request.ProviderType == ProviderType.Business && string.IsNullOrWhiteSpace(request.LegalName))
            throw new BusinessRuleException("A business needs its legal name.");
    }

    private static void Apply(ParkingProvider provider, ProviderRequest request)
    {
        provider.ProviderType = request.ProviderType;
        provider.LegalName = request.LegalName;
        provider.TaxNumber = request.TaxNumber;
        provider.Description = request.Description;
        provider.ContactEmail = request.ContactEmail;
        provider.ContactPhone = request.ContactPhone;
    }

    private static ProviderResponse ToResponse(ParkingProvider p) => new(
        p.Id, p.ProviderType, p.DisplayName, p.LegalName, p.TaxNumber, p.Description,
        p.ContactEmail, p.ContactPhone, p.VerificationStatus, p.CreatedAt);
}
