namespace GjejVend.Application.Common.Events;

// Shared event contracts. M2 and M3 communicate through these instead of referencing each other.

public interface IDomainEvent;

public sealed record PaymentCompleted(Guid BookingId, Guid PaymentId, decimal Amount, string Currency) : IDomainEvent;

public sealed record PaymentFailed(Guid BookingId, Guid PaymentId, string Reason) : IDomainEvent;

public sealed record BookingCancelled(Guid BookingId, DateTime CancelledAt, Guid? CancelledByUserId) : IDomainEvent;

public interface IEventSubscriber<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct);
}

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IDomainEvent;
}
