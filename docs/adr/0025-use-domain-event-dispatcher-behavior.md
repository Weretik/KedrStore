# ADR 0025: Use DomainEventDispatcherBehavior

## Date
2025-06-17

## Status
Accepted

## Context
DomainEventDispatcherBehavior is a custom MediatR pipeline behavior designed to handle domain events after a request is processed. It ensures that domain events generated during the execution of a request are dispatched to their respective handlers, maintaining the integrity of the domain layer and enabling event-driven communication.

## Decision
We decided to use DomainEventDispatcherBehavior in the project to:

1. Automatically dispatch domain events after a request is processed.
2. Ensure that domain events are handled consistently across the application.
3. Decouple the domain layer from the application layer by using event-driven communication.
4. Align with DDD principles by propagating domain events to their handlers.

## Consequences
### Positive
1. Simplifies the handling of domain events by automating their dispatch.
2. Improves decoupling between the domain and application layers.
3. Ensures consistency in event handling across the application.
4. Enables asynchronous processing of domain events, improving scalability.

### Negative
1. Adds complexity to the MediatR pipeline by introducing custom behaviors.
2. Requires careful management of domain events to avoid unintended side effects.
3. May lead to performance issues if too many domain events are generated simultaneously.

## Example
DomainEventDispatcherBehavior is implemented as follows:

**DomainEventDispatcherBehavior.cs**:
```csharp
public class DomainEventDispatcherBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly DbContext _dbContext;

    public DomainEventDispatcherBehavior(
        IDomainEventDispatcher domainEventDispatcher,
        DbContext dbContext)
    {
        _domainEventDispatcher = domainEventDispatcher;
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        var entitiesWithEvents = _dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return response;
    }
}
```

**Registration in DI**:
```csharp
services.AddMediatR(cfg => {
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatcherBehavior<,>));
});
```
