using BuildingBlocks.Application.Notifications;
using BuildingBlocks.Domain.Abstractions;
using Mediator;
using BuildingBlocks.Domain.Abstractions.Events;

namespace BuildingBlocks.Infrastructure.DomainEvents;

public sealed class MediatorDomainEventDispatcher(IMediator mediator)
    : IDomainEventDispatcher
{
    public ValueTask DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        => mediator.Publish(new DomainEventNotification(domainEvent), cancellationToken);
}
