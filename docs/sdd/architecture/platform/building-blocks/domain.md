# BuildingBlocks.Domain

## Structure

~~~text
BuildingBlocks.Domain/
├── Abstractions/
│   ├── IEntity<TId>, IEntityId<T>
│   ├── IAggregateRoot
│   ├── IAuditableEntity, ISoftDelete
│   ├── IDomainEvent, IHasDomainEvents
│   └── IDomainError, DomainError
├── Entity/
│   ├── BaseEntity<TId>
│   └── BaseAuditableEntity<TId>
└── Exceptions/
    └── DomainException
~~~

## Existing patterns

- `BaseEntity<TId>` contains identity, same-concrete-type and non-transient-ID equality, and methods for accumulating domain events.
- `BaseAuditableEntity<TId>` adds `CreatedAt`, `UpdatedAt`, `IsDeleted`, and explicit change-marking methods.
- `DomainError` is the standard error value with a code and message.
- `DomainException` carries `IDomainError`; `ExceptionBehavior` maps it to `Result.Invalid` for handlers that return Result.
- `IAggregateRoot` marks aggregate roots; `IHasDomainEvents` defines the event lifecycle contract.

## Usage

Inherit an aggregate/entity from the corresponding base only when its ID and event lifecycle fit. Create an event through `AddDomainEvent` after a domain operation is complete. Do not dispatch it in Domain.

Do not add EF attributes, HTTP concepts, repository implementations, or transport-layer DTOs here.
