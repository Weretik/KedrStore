namespace Infrastructure.Common.Events;

public sealed class MediatorDomainEventDispatcher(IMediator mediator)
    : IDomainEventDispatcher
{
    public ValueTask DispatchAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        var notifType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        var notif = Activator.CreateInstance(notifType, domainEvent)!;

        return mediator.Publish((INotification)notif, ct);
    }

    private ValueTask PublishDynamic<TEvent>(
        DomainEventNotification<TEvent> notification, CancellationToken ct)
        where TEvent : IDomainEvent
        => mediator.Publish(notification, ct);
}
