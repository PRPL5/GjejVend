namespace GjejVend.Domain.Bookings;

public static class BookingTransitions
{
    private static readonly Dictionary<BookingStatus, BookingStatus[]> Allowed = new()
    {
        [BookingStatus.Pending] = [BookingStatus.Confirmed, BookingStatus.Cancelled, BookingStatus.Expired],
        [BookingStatus.Confirmed] = [BookingStatus.Active, BookingStatus.Cancelled, BookingStatus.NoShow],
        [BookingStatus.Active] = [BookingStatus.Completed],
    };

    public static bool IsAllowed(BookingStatus from, BookingStatus to) =>
        Allowed.TryGetValue(from, out var next) && next.Contains(to);
}
