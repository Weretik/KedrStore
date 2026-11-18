# Platform 009: Read cache (HybridCache, in memory)

**Status:** accepted 2026-11-18, implemented the same day (owner: «добавь где реально было бы полезно … архитектурно верно … зафиксируй в доках»). **Owner:** platform (port and adapter in BuildingBlocks, the cached read in Catalog). **Decision:** [ADR-0016](../../../architecture/decisions/0016-read-cache-hybridcache.md).

## Goal

A read that is expensive, the same for every caller, and changes rarely is answered from memory, without letting the cache show wrong data for long and without each handler inventing its own caching.

## Where a cache is useful (checked 2026-11-18)

| Read | Cost per call | Callers | Decision |
| --- | --- | --- | --- |
| Public category tree `GET /api/{lang}/categories` | all categories + `COUNT` over the whole product read table, then the tree | site (each isolate, each revalidation), mobile app, anonymous | **cached**, 2 min |
| Category page, product lists, product page | one indexed query, paged or by slug; many different keys | site, mobile | not cached: cheap, low hit rate |
| Site SEO settings, contacts, requisites, home blocks, gallery, page SEO | one small row | site only, which caches them 10 min | not cached: no gain |
| Client prices, orders, customers | per user, money | admin, mobile | never cached |

## Rules

- R-01 Caching goes only through `IReadCache` (BuildingBlocks.Application); only `BuildingBlocks.Infrastructure/Caching` uses `HybridCache`, `IMemoryCache` or `IDistributedCache`.
- R-02 A cached value is an immutable sealed read model with `[ImmutableObject(true)]`; never an aggregate or other domain entity (checked by `ArchitectureTests/Application/CachedReadModelTests`), a `Result`, or per-user data.
- R-03 Every cached read has a `ReadCacheEntry`: stable name, key with every argument that changes the answer, one tag, a lifetime.
- R-04 Commands that change cached data implement `IInvalidatesReadCache`; the cache is dropped only after success.
- R-05 Invalidation reaches only the instance that ran the command; other instances and `Host.Jobs` changes show after the lifetime, so lifetimes are short.
- R-06 Each read tags the request span with `cache.<name> = hit | miss`.
- R-07 L2 is off; adding it is one host registration of an `IDistributedCache`, with no change to queries or commands.

## Scenarios

- SC-01 Two calls of the tree in one language within 2 minutes: the second answers without a database query; the span shows `cache.catalog.category-tree = hit`.
- SC-02 Ukrainian and Russian trees are cached apart.
- SC-03 An Admin hides a category: after the save succeeds, the next tree on that instance is rebuilt and no longer shows it.
- SC-04 An Admin changes a category slug: same as SC-03. A product slug change does not drop the tree.
- SC-05 A failed command (validation, not found) does not drop the cache.
- SC-06 The 1C import changes product counts: the tree shows them within 2 minutes.
- SC-07 Many requests hit a missing tree at once: one rebuild, the rest wait for it.

## File-role map

| File | Role |
| --- | --- |
| `BuildingBlocks.Application/Caching/IReadCache.cs` | port |
| `BuildingBlocks.Application/Caching/ReadCacheEntry.cs` | name, key, tag, lifetime (`readonly record struct`, `Create`) |
| `BuildingBlocks.Application/Caching/IInvalidatesReadCache.cs` | command marker with tags |
| `BuildingBlocks.Application/Behaviors/ReadCacheInvalidationBehavior.cs` | drops tags after a successful command |
| `BuildingBlocks.Infrastructure/Caching/HybridReadCache.cs` | adapter over `HybridCache`, span tag |
| `BuildingBlocks.Infrastructure/Caching/ReadCacheRegistration.cs` | `AddReadCache()` |
| `Host.Api/ModulesRegistrationExtensions.cs`, `Host.Api/Application/MediatorRegistrationExtensions.cs` | composition |
| `Catalog.Application/Features/Categories/Shared/Tree/CategoryTreeCache.cs` | entry and tag of the tree |
| `Catalog.Application/Features/Categories/Shared/Tree/CategoryTreeSnapshot.cs` | cached read model |
| `GetCategoriesHandler.cs`, `SetCategorySiteVisibilityCommand.cs`, `ChangeSlugCommand.cs` | use and invalidation |

## Tests and evidence

- Unit: [ReadCacheInvalidationBehaviorTests](../../../../../tests/UnitTests/BuildingBlocks/Caching/ReadCacheInvalidationBehaviorTests.cs) (SC-05), [ReadCacheEntryTests](../../../../../tests/UnitTests/BuildingBlocks/Caching/ReadCacheEntryTests.cs), [CategoryPresentationApplicationTests](../../../../../tests/UnitTests/Catalog/Application/CategoryPresentationApplicationTests.cs) (SC-01–SC-04).
- Integration (no Docker): [HybridReadCacheTests](../../../../../tests/IntegrationTests/Platform/Caching/HybridReadCacheTests.cs), [ReadCacheTraceTests](../../../../../tests/IntegrationTests/Platform/Caching/ReadCacheTraceTests.cs) (SC-01, R-06).
- Architecture: [ReadCacheTests](../../../../../tests/ArchitectureTests/Application/ReadCacheTests.cs) (R-01).
- 2026-11-18: the focused unit, integration and all architecture tests pass locally.
