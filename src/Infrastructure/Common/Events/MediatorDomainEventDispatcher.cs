namespace Infrastructure.Common.Events;

public sealed class MediatorDomainEventDispatcher(IMediator mediator)
    : IDomainEventDispatcher
{
    public ValueTask DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        => mediator.Publish(new DomainEventNotification(domainEvent), cancellationToken);
}
