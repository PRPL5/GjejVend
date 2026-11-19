using GjejVend.Domain.Identity;

namespace GjejVend.Domain.Bookings;

public sealed class Booking
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid LocationId { get; set; }
    public Guid? SpaceId { get; set; }
    public Guid? VehicleId { get; set; }
    public required string VehiclePlate { get; set; }
    public VehicleType VehicleType { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "EUR";
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal ProviderAmount { get; set; }
    public string PricingSnapshot { get; set; } = "{}";
    public string TicketToken { get; set; } = string.Empty;
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public Guid? CancelledBy { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
