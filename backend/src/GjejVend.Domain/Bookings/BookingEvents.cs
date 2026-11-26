namespace GjejVend.Domain.Bookings;

public abstract record BookingEvent(Guid BookingId, Guid CustomerId, Guid LocationId, string Reference);

public sealed record BookingCreated(Guid BookingId, Guid CustomerId, Guid LocationId, string Reference)
    : BookingEvent(BookingId, CustomerId, LocationId, Reference);

public sealed record BookingConfirmed(Guid BookingId, Guid CustomerId, Guid LocationId, string Reference)
    : BookingEvent(BookingId, CustomerId, LocationId, Reference);

public sealed record BookingCancelled(Guid BookingId, Guid CustomerId, Guid LocationId, string Reference, Guid CancelledBy, string? Reason, DateTime CancelledAt)
    : BookingEvent(BookingId, CustomerId, LocationId, Reference);

public sealed record BookingExtended(Guid BookingId, Guid CustomerId, Guid LocationId, string Reference, DateTime EndsAt)
    : BookingEvent(BookingId, CustomerId, LocationId, Reference);

public sealed record BookingCheckedIn(Guid BookingId, Guid CustomerId, Guid LocationId, string Reference)
    : BookingEvent(BookingId, CustomerId, LocationId, Reference);

public sealed record BookingCompleted(Guid BookingId, Guid CustomerId, Guid LocationId, string Reference)
    : BookingEvent(BookingId, CustomerId, LocationId, Reference);

public sealed record BookingNoShow(Guid BookingId, Guid CustomerId, Guid LocationId, string Reference)
    : BookingEvent(BookingId, CustomerId, LocationId, Reference);
