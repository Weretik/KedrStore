namespace BuildingBlocks.Application.Notifications;

public interface IDomainEventDispatcher
{
    ValueTask DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
