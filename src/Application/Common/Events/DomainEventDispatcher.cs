using IDomainEvent = Application.Common.Abstractions.Events.IDomainEvent;

namespace Application.Common.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly List<IDomainEvent> _domainEvents;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
        _domainEvents = new List<IDomainEvent>();
    }

    public async Task DispatchAsync(CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in _domainEvents.ToList())
        {
            await DispatchAsync(domainEvent, cancellationToken);
            _domainEvents.Remove(domainEvent);
        }
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _mediator.Publish(domainEvent, cancellationToken);
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }

    // Можно добавить метод для регистрации событий, если требуется
    public void RegisterEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}

