# ADR 0021: Use Domain Events

## Date
2025-06-17

## Status
Accepted

## Context
Domain events are a design pattern used to notify other parts of the system about changes or actions that have occurred within the domain. They help decouple different parts of the application and ensure that business logic remains cohesive and focused. In the Kedr E-Commerce Platform, domain events are used to propagate changes in the domain layer to other layers, such as the application or infrastructure layers.

## Decision
We decided to use domain events in the project to:

1. Decouple different parts of the system by using event-driven communication.
2. Ensure that business logic remains cohesive and focused within the domain layer.
3. Enable asynchronous processing of events to improve scalability and responsiveness.
4. Align with DDD principles by defining domain events as part of the domain layer.

## Consequences
### Positive
1. Improves decoupling between different parts of the system.
2. Enables asynchronous processing, improving scalability and responsiveness.
3. Simplifies the propagation of changes across layers.
4. Aligns with DDD principles, ensuring a clean and maintainable architecture.

### Negative
1. Adds complexity to the system by introducing event handling mechanisms.
2. Requires careful management of event handlers to avoid unintended side effects.
3. May lead to performance issues if too many events are generated or processed simultaneously.

## Example
Domain events are implemented as classes inheriting from `IDomainEvent` in the domain layer. For example:

**ProductCreatedEvent.cs**:
```csharp
public class ProductCreatedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Product Product { get; }

    public ProductCreatedEvent(Product product)
    {
        Product = product;
    }
}
```

**Usage in Domain Entity**:
```csharp
public class Product
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Handling Events in Application Layer**:
```csharp
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Handle the event (e.g., send an email, update a cache, etc.)
        return Task.CompletedTask;
    }
}
```
