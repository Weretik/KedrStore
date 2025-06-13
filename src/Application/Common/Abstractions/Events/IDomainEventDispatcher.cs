namespace Application.Common.Abstractions.Events
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(CancellationToken cancellationToken = default);
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
