# Read cache (HybridCache)

**Static code check:** 2026-11-18. **Owner:** platform ([`BuildingBlocks.Application/Caching`](../../../../src/BuildingBlocks/BuildingBlocks.Application/Caching/), [`BuildingBlocks.Infrastructure/Caching`](../../../../src/BuildingBlocks/BuildingBlocks.Infrastructure/Caching/), wired in `Host.Api`).

Expensive reads that are the same for everyone are kept in the memory of the API instance for a short time. Today only the public category tree is cached (2 minutes per language). Commands that change it drop it at once on their instance; other instances and the 1C import show changes after the lifetime. There is no Redis: a shared L2 can be added later by registering an `IDistributedCache` in the host.

## How it works

1. **Registration.** [`AddReadCache()`](../../../../src/BuildingBlocks/BuildingBlocks.Infrastructure/Caching/ReadCacheRegistration.cs) registers .NET `HybridCache` and the port `IReadCache` → [`HybridReadCache`](../../../../src/BuildingBlocks/BuildingBlocks.Infrastructure/Caching/HybridReadCache.cs); [`ModulesRegistrationExtensions`](../../../../src/Bootstrapper/Host.Api/ModulesRegistrationExtensions.cs) calls it, and [`MediatorRegistrationExtensions`](../../../../src/Bootstrapper/Host.Api/Application/MediatorRegistrationExtensions.cs) adds [`ReadCacheInvalidationBehavior`](../../../../src/BuildingBlocks/BuildingBlocks.Application/Behaviors/ReadCacheInvalidationBehavior.cs) to the pipeline.
2. **Read.** A query handler asks `IReadCache.GetOrCreateAsync(entry, read)`. [`ReadCacheEntry`](../../../../src/BuildingBlocks/BuildingBlocks.Application/Caching/ReadCacheEntry.cs) gives the name, key, tag and lifetime. On a miss the read runs once (other callers wait) and the answer, an immutable read model, is kept.
3. **Invalidation.** A command implementing [`IInvalidatesReadCache`](../../../../src/BuildingBlocks/BuildingBlocks.Application/Caching/IInvalidatesReadCache.cs) lists tags; after it succeeds the behavior drops them.
4. **Tracing.** Each read sets `cache.<name>` = `hit` or `miss` on the request span in Cloud Trace.

## Cached reads

| Read | Entry | Lifetime | Dropped by |
| --- | --- | --- | --- |
| Public category tree ([`GetCategoriesHandler`](../../../../src/Modules/Catalog/Catalog.Application/Features/Categories/Queries/GetCategories/GetCategoriesHandler.cs)) | [`CategoryTreeCache`](../../../../src/Modules/Catalog/Catalog.Application/Features/Categories/Shared/Tree/CategoryTreeCache.cs) `catalog.category-tree:<lang>:<type>`, tag `catalog:category-tree` | 2 min | `SetCategorySiteVisibilityCommand`, `ChangeSlugCommand` for a category |

## Code structure

```text
src/BuildingBlocks/
├── BuildingBlocks.Application/
│   ├── Caching/
│   │   ├── IReadCache.cs                # port
│   │   ├── ReadCacheEntry.cs            # name, key, tag, lifetime
│   │   └── IInvalidatesReadCache.cs     # command marker
│   └── Behaviors/ReadCacheInvalidationBehavior.cs
└── BuildingBlocks.Infrastructure/Caching/
    ├── HybridReadCache.cs               # HybridCache adapter, span tag
    └── ReadCacheRegistration.cs         # AddReadCache()
```

## Tests

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Caching|FullyQualifiedName~CategoryPresentationApplicationTests"`.
- Integration (no Docker): `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~Platform.Caching"`.
- Architecture: `ReadCacheTests` (only the adapter uses cache libraries).

## Related sources

- [Specification](../../specs/platform/009-read-cache/README.md), [ADR-0016](../../architecture/decisions/0016-read-cache-hybridcache.md), [backend rules: caching reads](../../standards/backend-rules.md#caching-reads)
