# System map

## Executable hosts

- src/Bootstrapper/Host.Api composes DI, middleware, controllers, authentication and OpenAPI.
- src/Bootstrapper/Host.Jobs/Host.Jobs runs scheduled/background integration work.

## Business modules

- Catalog owns categories, products, translations, prices, projections and 1C catalogue synchronization.
- Sales owns sales customers, counterparties, pricing policy and sales-facing catalogue reads.
- Identity owns ASP.NET Identity, roles, policies, current-user/session integration and seeders.
- BuildingBlocks owns shared abstractions and cross-cutting implementation; it must not acquire business rules.

## Dependency direction

~~~text
Host.Api / Host.Jobs
        │
        ├── Module.Api ──► Module.Application ──► Module.Domain
        └── Module.Infrastructure ─────────────► Module.Application + Module.Domain

BuildingBlocks.* supplies shared primitives; it does not depend on business modules.
~~~

There is no direct dependency from Domain to Application or Infrastructure, and no Application dependency on a concrete DbContext, HTTP client, scheduler or EF repository.
