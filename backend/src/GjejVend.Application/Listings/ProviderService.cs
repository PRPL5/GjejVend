using GjejVend.Application.Common;
using GjejVend.Domain.Identity;
using GjejVend.Domain.Listings;
using GjejVend.Infrastructure.Listings;

namespace GjejVend.Application.Listings;

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
    Task<ProviderResponse> CreateAsync(ProviderRequest request, CancellationToken ct = default);
    Task<ProviderResponse> GetMineAsync(CancellationToken ct = default);
    Task<ProviderResponse> UpdateMineAsync(ProviderRequest request, CancellationToken ct = default);
}

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
