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

## Available approaches

- BaseEntity<TId> provides identity, equality by concrete type plus non-transient ID, and domain-event collection methods.
- BaseAuditableEntity<TId> adds CreatedAt, UpdatedAt, IsDeleted and explicit mark methods.
- DomainError is the standard code/message error value.
- DomainException carries an IDomainError. ExceptionBehavior maps it to Result.Invalid for Result-based handlers.
- IAggregateRoot is a marker for aggregate roots; IHasDomainEvents is the event lifecycle contract.

## Use

Derive an aggregate/entity from the appropriate base only when its ID and event lifecycle fit. Raise an event with AddDomainEvent inside a completed domain operation. Do not dispatch it in Domain.

Do not add EF attributes, HTTP concepts, repository implementations or transport DTOs here.
