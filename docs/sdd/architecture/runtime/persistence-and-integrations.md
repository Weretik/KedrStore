# Data and integrations

## Persistence

Catalog, Sales, and Identity use separate EF Core contexts on PostgreSQL. Context configuration, migrations, and entity configurations stay in the responsible module's Infrastructure layer.

- Use `AsNoTracking` for read-only queries.
- Use projections and server-side pagination; avoid N+1 queries and full materialization for paged lists.
- Use repositories and Ardalis.Specification where the module already follows that approach.
- `IUnitOfWork` remains the transaction-boundary abstraction for coordinated writes.
- Do not add a migration or change schema or concurrency semantics without an explicit feature requirement.

## External adapters

1C clients, Telegram notifications, Excel export, cache and storage adapters, and background job implementations belong in Infrastructure. Application owns only abstractions and use-case coordination.

Background work should use the existing `IBackgroundJobService` abstraction or the registered job approach. Do not bind Application code directly to the scheduler. A separate `Host.Jobs` runs one operation and exits.

## Read models

Catalog product-list endpoints use `ProductListProjection` for optimized read queries. Rebuilding the projection belongs to Infrastructure and runs within the current synchronization cycle. The projection is a read model, not the aggregate that owns product rules.
