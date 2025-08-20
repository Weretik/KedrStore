namespace Application.Common.Notifications;

public interface IDomainEventDispatcher
{
    ValueTask DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
