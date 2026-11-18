# ADR-0016: Expensive public reads are cached in memory through one port over HybridCache

- Status: accepted
- Date: 2026-11-18 (owner's request: «добавь кэш, только архитектурно верно и там, где реально полезно»)
- Owner: Виталий (project owner)

## Context

Most reads are cheap single-row or paged queries, and the public site (Next.js on Cloudflare) already keeps its own data cache for 10 minutes ([kedr-web site cache](https://github.com/Weretik/kedr-web/blob/main/docs/architecture/site/README.md)). One public read is still expensive and identical for every caller: the category tree with product counts (`GET /api/{lang}/categories`) reads every category and counts the whole product read table on each call. The mobile app has no site cache in front of it, every site isolate and every revalidation calls it, and the endpoint is anonymous, so anyone can make the server rebuild it on every request.

The API runs on Cloud Run, which may start several instances; the 1C import runs in `Host.Jobs`, another process.

## Alternatives considered

- Redis (Memorystore) as a shared cache: rejected for now. It costs money every month, adds a network hop and an outage point, and the load is small. Kept possible as an L2 (see Decision).
- `IMemoryCache` in each handler: rejected. Every handler would invent its own key, lifetime and invalidation; there is no protection against many callers rebuilding one missing key at once, and no single place for tracing or a later L2.
- Output caching of HTTP responses: rejected. It sits in the API layer, cannot be dropped by a command in another layer without leaking HTTP concerns into Application, and caches per URL instead of per piece of data.
- Caching inside a Mediator behavior keyed by the query: rejected. A query answers a `Result`, which must not be cached on failure, and a generic key from the query object hides what is cached and for how long.

## Decision

- One port `IReadCache` in `BuildingBlocks.Application/Caching` with `GetOrCreateAsync(ReadCacheEntry, read)` and `InvalidateAsync(tag)`. `ReadCacheEntry` (a `readonly record struct` built by `Create`) holds a stable name, the key (name plus the read's arguments), one tag and the lifetime.
- One adapter `HybridReadCache` in `BuildingBlocks.Infrastructure/Caching` over .NET `HybridCache` (`Microsoft.Extensions.Caching.Hybrid`): concurrent callers of a missing key wait for one read (stampede protection), entries carry tags, and each read tags the request span in Cloud Trace with `cache.<name> = hit | miss`. `AddReadCache()` registers both; `Host.Api` calls it.
- **L1 only today.** No `IDistributedCache` is registered, so HybridCache keeps answers in the memory of the instance. To add a shared L2 later, register one in the host (for example `AddStackExchangeRedisCache`); HybridCache picks it up and no query or command changes.
- **Only queries cache, only read models.** A query handler wraps its expensive read in `IReadCache`; the cached value is an immutable, sealed read model marked `[ImmutableObject(true)]` (HybridCache then hands out the same instance instead of copying it). Aggregates, other domain entities, `Result` objects and per-user data are never cached; `CachedReadModelTests` fails when an entity is reachable from a cached type. Domain knows nothing about caching.
- **Commands drop the cache by tag.** A command whose data a cached read shows implements `IInvalidatesReadCache`; `ReadCacheInvalidationBehavior` drops its tags after the command succeeds. A write outside Mediator calls `IReadCache.InvalidateAsync`.
- **Where it is used now:**
  - the public category tree ([spec platform/009](../../specs/platform/009-read-cache/README.md)), 2 minutes, tag `catalog:category-tree`, dropped by the «show on site» mark and a category slug change;
  - the admin site models state of the whole catalog (`SiteModelStateReader`, added 2026-11-18 to close database rule 1.4): «Моделі», «Вітрина сайту», its category tree and «Огляд» rank and count every on-site product, so they share one snapshot for 1 minute, tag `catalog:site-models`, dropped by every Catalog command except the swatch upload and category SEO texts (`ReadCacheTests.EveryCatalogCommand_DropsTheSiteModelsState`). One model and the model commands read only their models, uncached. The cached value is `SiteModelSnapshot` (plain rows, serializable); each request indexes it in memory.
- Only `BuildingBlocks.Infrastructure/Caching` may use `HybridCache`, `IMemoryCache` or `IDistributedCache`; `ReadCacheTests` enforces it.

## Consequences

- Invalidation is local: a command drops the tag on the instance that ran it. Other API instances, and changes made by `Host.Jobs` (the 1C import), show after the entry's lifetime. So every cached read has a short lifetime chosen for what a stale answer costs, and money, stock, orders and prices in an order are never cached.
- Memory: each entry lives in the instance; answers larger than 16 MB are not cached (raised from 4 MB on 2026-11-18 for the site models state of the whole catalog).
- A new cached read needs a reason recorded in its spec (cost, callers, how stale it may be), a tag, and the commands that drop it.

## Sources and checks

- `src/BuildingBlocks/BuildingBlocks.Application/Caching/`, `src/BuildingBlocks/BuildingBlocks.Application/Behaviors/ReadCacheInvalidationBehavior.cs`, `src/BuildingBlocks/BuildingBlocks.Infrastructure/Caching/`.
- Tests: `tests/UnitTests/BuildingBlocks/Caching/`, `tests/IntegrationTests/Platform/Caching/`, `tests/ArchitectureTests/Application/ReadCacheTests.cs`.
