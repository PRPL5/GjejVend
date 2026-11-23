using GjejVend.Application.Common;
using GjejVend.Application.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace GjejVend.Infrastructure.Events;

/// <summary>Publishes events to every registered <see cref="IEventSubscriber{TEvent}"/> in the same scope.</summary>
public sealed class InProcessEventPublisher(IServiceProvider services) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IDomainEvent
    {
        foreach (var subscriber in services.GetServices<IEventSubscriber<TEvent>>())
            await subscriber.HandleAsync(@event, ct);
    }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
