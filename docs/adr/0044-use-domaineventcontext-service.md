# ADR 0044: Use DomainEventContext Service

## Date
2025-06-21

## Status
Accepted

## Context
The `DomainEventContext` service is used to manage domain events within the application. It provides methods to retrieve entities with domain events and clear those events after processing. This ensures proper handling of domain events and avoids unintended side effects.

## Decision
We decided to use the `DomainEventContext` service for managing domain events instead of directly interacting with entities or DbContext.

## Examples
### Positive Example
```csharp
var domainEventContext = new DomainEventContext(dbContexts);
var domainEntities = domainEventContext.GetDomainEntities();
domainEventContext.ClearDomainEvents();
```

### Negative Example
```csharp
var domainEntities = dbContexts
    .SelectMany(db => db.ChangeTracker
        .Entries<IHasDomainEvents>()
        .Where(e => e.Entity.DomainEvents.Any())
        .Select(e => e.Entity));

foreach (var entity in domainEntities)
    entity.ClearDomainEvents();
```

## Consequences
### Positive
- Centralized handling of domain events.
- Simplifies the process of clearing domain events.
- Improves maintainability and readability.

### Negative
- Adds dependency on the `DomainEventContext` service.
- Requires developers to use the service instead of direct DbContext operations.
