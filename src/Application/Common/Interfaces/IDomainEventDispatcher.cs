namespace Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    ValueTask DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
