# Domain Events System in KedrStore

## Overview

The domain events system in KedrStore implements the Domain Events pattern, enabling different parts of the application to react to significant domain changes without direct dependencies. This ensures loose coupling of components and aligns with Domain-Driven Design (DDD) principles.

## Key Benefits

1. **Separation of concerns** – each event handler is responsible for a specific task
2. **Reduced coupling** – components do not need direct references to each other
3. **Improved testability** – easy to test individual event handlers
4. **Simplified maintenance** – new features can be added without modifying existing code
5. **Asynchronous processing** – supports async event handling
6. **Business transparency** – events reflect meaningful domain changes

## Architecture of the Domain Event System

### Main Components

1. **IDomainEvent** – base interface for all domain events
2. **DomainEvent** – abstract base class for domain events
3. **IDomainEventHandler<T>** – interface for event handlers
4. **IDomainEventDispatcher** – interface for the event dispatcher
5. **IHasDomainEvents** – interface for entities that can emit events
6. **DomainEventDispatcherBehavior** – MediatR pipeline behavior for automatic event dispatching

### Component Interaction Diagram

```
┌───────────────┐    emits         ┌───────────────┐
│   Domain      │ ───────────────► │   Domain      │
│   Entity      │                  │    Event      │
└───────────────┘                  └───────┬───────┘
                                          │
                                          ▼
┌───────────────┐    dispatches    ┌───────────────┐
│   Event       │ ◄─────────────── │   Dispatcher  │
│   Handler     │                  │               │
└───────────────┘                  └───────────────┘
```

## Implementation

### Domain Event Interface

```csharp
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
```

### Domain Event Base Class

```csharp
public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }

    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
```

### Domain Event Handler Interface

```csharp
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
```

### Interface for Entities with Events

```csharp
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent domainEvent);
    void RemoveDomainEvent(IDomainEvent domainEvent);
    void ClearDomainEvents();
}
```

## Usage in the Project

### Creating a Domain Event

```csharp
public class ProductCreatedEvent : DomainEvent
{
    public int ProductId { get; }
    public string Name { get; }
    public decimal Price { get; }

    public ProductCreatedEvent(int productId, string name, decimal price)
    {
        ProductId = productId;
        Name = name;
        Price = price;
    }
}
```

### Emitting Events from Entity

```csharp
public class Product : Entity, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
        AddDomainEvent(new ProductCreatedEvent(Id, Name, Price));
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new InvalidOperationException("Price must be greater than zero");

        decimal oldPrice = Price;
        Price = newPrice;
        AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
    }

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Creating a Domain Event Handler

```csharp
public class NotifyAdminOnProductCreatedHandler : IDomainEventHandler<ProductCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<NotifyAdminOnProductCreatedHandler> _logger;

    public NotifyAdminOnProductCreatedHandler(
        IEmailService emailService,
        ILogger<NotifyAdminOnProductCreatedHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(ProductCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending notification for product: {ProductName} (ID: {ProductId})",
            domainEvent.Name, domainEvent.ProductId);

        await _emailService.SendEmailAsync(
            "admin@example.com",
            "New product created",
            $"A new product has been created: {domainEvent.Name} (ID: {domainEvent.ProductId}) with price {domainEvent.Price:C}",
            cancellationToken);
    }
}
```

### Domain Event Dispatcher Behavior

```csharp
public class DomainEventDispatcherBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IApplicationDbContext _dbContext;

    public DomainEventDispatcherBehavior(
        IDomainEventDispatcher domainEventDispatcher,
        IApplicationDbContext dbContext)
    {
        _domainEventDispatcher = domainEventDispatcher;
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response = await next();

        List<IDomainEvent> domainEvents = _dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        _dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents());

        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return response;
    }
}
```

## Dispatcher Registration

```csharp
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatcherBehavior<,>));
        });

        services.AddDomainEventHandlers(typeof(ApplicationAssemblyMarker).Assembly);
        return services;
    }

    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .Select(i => new { HandlerType = t, EventType = i.GetGenericArguments()[0] }));

        foreach (var handler in handlerTypes)
        {
            var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(handler.EventType);
            services.AddScoped(handlerInterfaceType, handler.HandlerType);
        }

        return services;
    }
}
```

## Event Dispatcher (Infrastructure)

```csharp
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        _logger.LogDebug("Dispatching event {EventType}", eventType.Name);

        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("HandleAsync");
            try
            {
                await (Task)method!.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing domain event {EventType}", eventType.Name);
            }
        }
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
```

## Best Practices

1. **When to Raise Events**:
   - On entity creation, update, or deletion
   - On state transitions
   - On major business milestones

2. **Naming**:
   - Use past tense: `ProductCreated`, `OrderShipped`, `PriceChanged`

3. **Event Payloads**:
   - Include entity IDs and key information
   - Do **not** include full entity objects

4. **Error Handling**:
   - Event handler errors should not disrupt the main operation flow
   - Always log exceptions in handlers

5. **Order of Execution**:
   - Events are processed in order of addition
   - Use ordinal values if strict ordering is required

## Conclusion

The domain events system is a vital part of the KedrStore architecture. It promotes loose coupling, enhances testability, and simplifies maintenance. When used properly, it allows the system to grow and evolve without changing the core logic.
