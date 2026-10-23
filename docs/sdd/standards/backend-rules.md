# Backend rules

## Layers and dependencies

Follow dependency direction `API → Application → Domain`. Infrastructure depends on inner layers, not the reverse.

- Domain contains business invariants, entities, value objects, and domain services; it contains no transport DTOs or Infrastructure dependencies.
- Application coordinates use cases through abstractions, not Infrastructure implementations.
- Infrastructure contains persistence and integration adapters; business rules do not belong there.
- API handles HTTP binding, authorization, and result mapping only; it contains no EF or business logic.

## Entity identifiers

Apply the [identifier strategy](identifier-strategy.md) before creating or changing an entity identifier. Do not default to `Guid`: the ID type depends on the creation lifecycle and integrations. In Domain, wrap the chosen scalar in a typed value object/readonly struct; do not use raw primitive IDs in `BaseEntity`, `BaseAuditableEntity`, or Domain foreign keys.

## CQRS, Mediator, and code structure

Group application code by feature. Keep a use case's command or query, handler, validator, and private DTOs together. Each Mediator handler owns one use case; commands change state, queries do not.

Each file and directory should have a coherent responsibility. Before creating files for a feature, compare the planned SDD map with current module boundaries; review the actual files before completion. Separate independent use cases, read/write contracts, and adapters by ownership without creating unnecessary abstractions or folders for a single small file. Line count is a reason to review a file, not an automatic split rule.

Pass `CancellationToken` through the entire flow. Keep cross-cutting concerns in Mediator pipeline behaviors, but do not move business rules there.

Use specific names such as `CreateOrderCommand`, `GetPublicProductListQuery`, and `GetAdminProductListQueryHandler`. Avoid vague folders or names such as `Common`, `Misc`, `Helper`, `Utils`, and `Manager`. Do not add abstractions without a concrete need.

## Domain events and transactions

Follow the current domain event lifecycle: `collect → dispatch → clear`. Use cases that change multiple aggregates coordinate a transaction through the existing `IUnitOfWork`.
