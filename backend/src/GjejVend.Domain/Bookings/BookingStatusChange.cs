namespace GjejVend.Domain.Bookings;

public sealed class BookingStatusChange
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public BookingStatus? FromStatus { get; set; }
    public BookingStatus ToStatus { get; set; }
    public Guid? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; }
}
