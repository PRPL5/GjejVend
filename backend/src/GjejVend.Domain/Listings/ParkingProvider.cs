namespace GjejVend.Domain.Listings;

public sealed class ParkingProvider
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public ProviderType ProviderType { get; set; }
    public required string DisplayName { get; set; }
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; }
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    /// <summary>Id of the provider's connected payout account (Stripe Connect). Null until they onboard.</summary>
    public string? PaymentAccountId { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
