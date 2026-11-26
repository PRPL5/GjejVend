using FluentValidation;
using GjejVend.Domain.Identity;
using GjejVend.Infrastructure.Bookings;

namespace GjejVend.Application.Bookings;

public sealed record CheckoutPreviewQuery(Guid LocationId, DateTimeOffset StartsAt, DateTimeOffset EndsAt);

public sealed record CheckoutPreview(
    Guid LocationId,
    string LocationName,
    string AddressLine,
    bool IsOpen,
    int FreeCapacity,
    decimal TotalAmount,
    string Currency,
    int Minutes,
    DefaultVehicle? DefaultVehicle);

public sealed record CreateBookingRequest(
    Guid LocationId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string VehiclePlate,
    VehicleType VehicleType,
    Guid? SpaceId = null,
    Guid? VehicleId = null);

public sealed record ExtendBookingRequest(DateTimeOffset EndsAt);

public sealed record CancelBookingRequest(string? Reason);

public sealed record TicketScanRequest(Guid LocationId, string Token);

public sealed record ArrivalsQuery(DateTimeOffset From, DateTimeOffset To);

public sealed record ReviewRequest(int Rating, string? Comment);

public sealed record ReviewResponseRequest(string Response);

internal sealed class CheckoutPreviewQueryValidator : AbstractValidator<CheckoutPreviewQuery>
{
    public CheckoutPreviewQueryValidator()
    {
        RuleFor(q => q.LocationId).NotEmpty();
        RuleFor(q => q.EndsAt).GreaterThan(q => q.StartsAt);
    }
}

internal sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(r => r.LocationId).NotEmpty();
        RuleFor(r => r.EndsAt).GreaterThan(r => r.StartsAt);
        RuleFor(r => r.VehiclePlate).NotEmpty().Matches("^[A-Za-z0-9][A-Za-z0-9 -]{1,14}$");
        RuleFor(r => r.VehicleType).IsInEnum();
    }
}

internal sealed class ExtendBookingRequestValidator : AbstractValidator<ExtendBookingRequest>
{
    public ExtendBookingRequestValidator() => RuleFor(r => r.EndsAt).NotEmpty();
}

internal sealed class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
{
    public CancelBookingRequestValidator() => RuleFor(r => r.Reason).MaximumLength(500);
}

internal sealed class TicketScanRequestValidator : AbstractValidator<TicketScanRequest>
{
    public TicketScanRequestValidator()
    {
        RuleFor(r => r.LocationId).NotEmpty();
        RuleFor(r => r.Token).NotEmpty().MaximumLength(64);
    }
}

internal sealed class ArrivalsQueryValidator : AbstractValidator<ArrivalsQuery>
{
    public ArrivalsQueryValidator() =>
        RuleFor(q => q.To)
            .GreaterThan(q => q.From)
            .Must((q, to) => to - q.From <= TimeSpan.FromDays(7))
            .WithMessage("The period can be at most 7 days.");
}

internal sealed class ReviewRequestValidator : AbstractValidator<ReviewRequest>
{
    public ReviewRequestValidator()
    {
        RuleFor(r => r.Rating).InclusiveBetween(1, 5);
        RuleFor(r => r.Comment).MaximumLength(1000);
    }
}

internal sealed class ReviewResponseRequestValidator : AbstractValidator<ReviewResponseRequest>
{
    public ReviewResponseRequestValidator() => RuleFor(r => r.Response).NotEmpty().MaximumLength(1000);
}
