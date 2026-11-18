# BuildingBlocks.Domain

## Structure

~~~text
BuildingBlocks.Domain/
├── Abstractions/
│   ├── IEntity<TId>, IEntityId<T>
│   ├── IAggregateRoot
│   ├── IAuditableEntity, ISoftDelete
│   └── ErrorType, DomainError
├── Entity/
│   ├── BaseEntity<TId>
│   └── BaseAuditableEntity<TId>
├── Identifiers/                 ids other modules store: UserId, MediaFileId
└── Results/
    └── DomainResult, DomainResult<T>, DomainChecks
~~~

## Existing patterns

- `BaseEntity<TId>` contains identity, same-concrete-type and non-transient-ID equality.
- `BaseAuditableEntity<TId>` adds `CreatedAt`, `UpdatedAt`, `IsDeleted`, and explicit change-marking methods.
- `DomainError(Code, Message, Type, Field)` is the one business error: a stable English code, a Ukrainian message without the field name, an `ErrorType` (`Validation`, `NotFound`, `Conflict`, `Forbidden`, `Unexpected`), and the input field when there is one.
- A business rule returns `DomainResult` or `DomainResult<T>`; `DomainChecks` collects several field checks and keeps the first error. The handler turns a failure into its use-case result with `ToResult()` from `BuildingBlocks.Application.Results`.
- Exceptions in Domain only report programming errors (`Argument*Exception`, `InvalidOperationException`); `ThrowIfFailure()` marks a check whose input the code built itself. `DomainErrorHandlingTests` enforces this.
- `IAggregateRoot` marks aggregate roots.
- `Identifiers/` holds the ids that more than one module stores ([ADR-0017](../../decisions/0017-shared-kernel-ids-across-modules.md)); `BuildingBlocks.Infrastructure` maps them with `AddKernelIds()`. An id used only inside its module stays beside its root.

## Usage

Inherit an aggregate/entity from the corresponding base only when its ID fits.

Do not add EF attributes, HTTP concepts, repository implementations, or transport-layer DTOs here.
