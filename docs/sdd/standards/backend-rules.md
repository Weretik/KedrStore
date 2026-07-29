# Backend rules

## Layers and dependencies

Follow the dependency direction `API → Application → Domain`. Infrastructure depends on the inner layers, never the reverse.

- Domain owns business invariants, entities, value objects, and domain services; it contains no transport DTOs or Infrastructure dependencies.
- Application orchestrates use cases through abstractions, not Infrastructure implementations.
- Infrastructure contains persistence and integration adapters; it does not own business rules.
- API performs only HTTP binding, authorization, and result mapping; it contains no EF or business logic.

## CQRS, Mediator, and code

Use feature-by-folder within Application. Keep the command/query, handler, validator, and private DTOs together by scenario. One Mediator handler owns one use case; commands change state, and queries do not.

Pass `CancellationToken` through the entire flow. Keep cross-cutting concerns in pipeline behaviors; do not move business rules into behaviors.

Use explicit names: `CreateOrderCommand`, `GetPublicProductListQuery`, `GetAdminProductListQueryHandler`. Do not create generic folders or names such as `Common`, `Misc`, `Helper`, `Utils`, or `Manager`. Do not add abstractions without a concrete need.

## Domain events and transactions

Follow the existing domain-event lifecycle: `collect → dispatch → clear`. Use cases with multiple aggregate changes coordinate the transaction through the existing `IUnitOfWork`.
