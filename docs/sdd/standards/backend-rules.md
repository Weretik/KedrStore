# Backend rules

## Layers and dependencies

Follow dependency direction `API → Application → Domain`. Infrastructure depends on inner layers, not the reverse.

- Domain contains business invariants, entities, value objects, and domain services; it contains no transport DTOs or Infrastructure dependencies.
- Application coordinates use cases through abstractions, not Infrastructure implementations.
- Infrastructure contains persistence and integration adapters; business rules do not belong there.
- API handles HTTP binding, authorization, and result mapping only; it contains no EF or business logic.

Choose the owning module, or propose a new one, by the [module rules](module-rules.md). They also fix the shape of a new module, its database schema, and FluentValidation use.

## Entity identifiers

Apply the [identifier strategy](identifier-strategy.md) before creating or changing an entity identifier. Do not default to `Guid`: the ID type depends on the creation lifecycle and integrations. In Domain, wrap the chosen scalar in a typed value object/readonly struct; do not use raw primitive IDs in `BaseEntity`, `BaseAuditableEntity`, or Domain foreign keys.

## CQRS, Mediator, and code structure

Group application code by feature. Keep a use case's command or query, handler, validator, and private DTOs together. Each Mediator handler owns one use case; commands change state, queries do not.

Each file and directory should have a coherent responsibility. Before creating files for a feature, compare the planned SDD map with current module boundaries; review the actual files before completion. Separate independent use cases, read/write contracts, and adapters by ownership without creating unnecessary abstractions or folders for a single small file. A folder holds at most six code files; past that, split it by meaning (the [folder axiom](module-rules.md#the-folder-axiom-at-most-six-files)). Line count is a reason to review a file, not an automatic split rule.

Pass `CancellationToken` through the entire flow. Keep cross-cutting concerns in Mediator pipeline behaviors, but do not move business rules there.

Use specific names such as `CreateOrderCommand`, `GetPublicProductListQuery`, and `GetAdminProductListHandler`. Avoid vague folders or names such as `Common`, `Misc`, `Helper`, `Utils`, and `Manager`. Do not add abstractions without a concrete need.

## Caching reads

Cache a read only when it is expensive, the same for every caller, and may be a little stale; write the reason, lifetime and invalidating commands into its spec ([ADR-0016](../architecture/decisions/0016-read-cache-hybridcache.md), [read cache](../features/platform/read-cache.md)). Use the `IReadCache` port with a `ReadCacheEntry`; never `IMemoryCache`, `IDistributedCache` or `HybridCache` outside `BuildingBlocks.Infrastructure/Caching` (an architecture test checks it). Cache only immutable sealed read models marked `[ImmutableObject(true)]`, never aggregates or other domain entities (`CachedReadModelTests`), `Result` objects, per-user data, money, stock or order prices. A command that changes cached data implements `IInvalidatesReadCache`. Invalidation is local to the instance, so keep lifetimes short (minutes).

## Transactions

The backend has no domain events (removed 2026-11-17; the outbox covers cross-module and outside messages). Use cases that change multiple aggregates coordinate a transaction through the existing `IUnitOfWork`.
