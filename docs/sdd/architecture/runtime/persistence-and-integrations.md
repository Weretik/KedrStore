# Persistence and integrations

## Persistence

Catalog, Sales and Identity use separate EF Core contexts backed by PostgreSQL. Context configuration, migrations and entity configurations stay in the owning Infrastructure project.

- Use AsNoTracking for read-only queries.
- Use projections and server-side paging; avoid N+1 queries and full materialization for pageable endpoints.
- Repositories and Ardalis.Specification are used where the module already uses them.
- IUnitOfWork remains the transaction-boundary abstraction for coordinated writes.
- Do not add a migration, change schema or change concurrency semantics without an explicit feature requirement.

## External adapters

1C clients, Telegram notification, Excel export, cache/storage adapters and background job implementations live in Infrastructure. Application owns only the abstraction and use-case orchestration.

Background work must use the existing IBackgroundJobService abstraction or registered job pattern. Do not couple application code directly to a scheduler.

## Read models

Catalog product list endpoints use ProductListProjection for read-optimised querying. Projection rebuilding belongs to Infrastructure and is triggered by the existing synchronization lifecycle. Treat projections as a read model, not the aggregate that owns product rules.
