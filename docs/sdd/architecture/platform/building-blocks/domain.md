# BuildingBlocks.Domain

## Structure

~~~text
BuildingBlocks.Domain/
├── Abstractions/
│   ├── IEntity<TId>, IEntityId<T>
│   ├── IAggregateRoot
│   ├── IAuditableEntity, ISoftDelete
│   ├── IDomainEvent, IHasDomainEvents
│   └── ErrorType, DomainError
├── Entity/
│   ├── BaseEntity<TId>
│   └── BaseAuditableEntity<TId>
└── Results/
    └── DomainResult, DomainResult<T>, DomainChecks
~~~

## Existing patterns

- `BaseEntity<TId>` contains identity, same-concrete-type and non-transient-ID equality, and methods for accumulating domain events.
- `BaseAuditableEntity<TId>` adds `CreatedAt`, `UpdatedAt`, `IsDeleted`, and explicit change-marking methods.
- `DomainError(Code, Message, Type, Field)` is the one business error: a stable English code, a Ukrainian message without the field name, an `ErrorType` (`Validation`, `NotFound`, `Conflict`, `Forbidden`, `Unexpected`), and the input field when there is one.
- A business rule returns `DomainResult` or `DomainResult<T>`; `DomainChecks` collects several field checks and keeps the first error. The handler turns a failure into its use-case result with `ToResult()` from `BuildingBlocks.Application.Results`.
- Exceptions in Domain only report programming errors (`Argument*Exception`, `InvalidOperationException`); `ThrowIfFailure()` marks a check whose input the code built itself. `DomainErrorHandlingTests` enforces this.
- `IAggregateRoot` marks aggregate roots; `IHasDomainEvents` defines the event lifecycle contract.

## Usage

Inherit an aggregate/entity from the corresponding base only when its ID and event lifecycle fit. Create an event through `AddDomainEvent` after a domain operation is complete. Do not dispatch it in Domain.

Do not add EF attributes, HTTP concepts, repository implementations, or transport-layer DTOs here.
