using GjejVend.Domain.Common;
using Npgsql;

namespace GjejVend.Infrastructure.Persistence;

internal static class DatabaseErrors
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        ["booking_invalid_period"] = "The booking period is not valid.",
        ["booking_customer_inactive"] = "Your account cannot make bookings.",
        ["booking_location_unavailable"] = "This parking is not available for booking.",
        ["booking_space_mismatch"] = "Choose a space only for parkings that sell spaces.",
        ["booking_vehicle_not_owned"] = "This vehicle does not belong to you.",
        ["booking_vehicle_type_not_allowed"] = "This parking does not accept this vehicle type.",
        ["booking_outside_opening_hours"] = "The parking is closed for part of this period.",
        ["booking_space_unavailable"] = "This space is already booked for this period.",
        ["booking_no_capacity"] = "The parking is full for this period.",
        ["booking_no_rate"] = "This parking has no price for this period.",
        ["booking_duration_out_of_range"] = "This booking is too short or too long for this parking.",
        ["booking_not_extendable"] = "Only confirmed or active bookings can be extended.",
        ["booking_invalid_transition"] = "The booking changed status in the meantime.",
        ["review_booking_not_eligible"] = "Only completed bookings can be reviewed.",
        ["review_already_exists"] = "This booking already has a review.",
    };

    public static async Task<T> GuardAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception) when (Translate(exception) is { } translated)
        {
            throw translated;
        }
    }

    public static Task GuardAsync(Func<Task> action) =>
        GuardAsync(async () =>
        {
            await action();
            return true;
        });

    private static DomainException? Translate(Exception exception)
    {
        if ((exception as PostgresException ?? exception.InnerException as PostgresException) is not { } error)
        {
            return null;
        }

        var code = error.SqlState switch
        {
            PostgresErrorCodes.RaiseException => error.MessageText,
            PostgresErrorCodes.CheckViolation when error.MessageText.StartsWith("booking_invalid_transition", StringComparison.Ordinal) => "booking_invalid_transition",
            PostgresErrorCodes.ExclusionViolation => "booking_space_unavailable",
            PostgresErrorCodes.UniqueViolation when error.ConstraintName == "ix_reviews_booking_id" => "review_already_exists",
            _ => null,
        };

        if (code == "booking_not_found")
        {
            return new NotFoundException(code, "Booking not found.");
        }

        return code is not null && Messages.TryGetValue(code, out var message) ? new ConflictException(code, message, error) : null;
    }
}
