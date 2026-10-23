# System map

## Executable hosts

- `src/Bootstrapper/Host.Api` composes DI, middleware, controllers, authentication, and OpenAPI.
- `src/Bootstrapper/Host.Jobs/Host.Jobs` runs one explicitly selected `--job=...` command and exits. An external scheduler or CI can invoke this host; it has no persistent internal scheduler. The `--job=migrate` command runs migrations and seeders.

## Business modules

- Catalog owns categories, products, translations, prices, projections, and catalog synchronization with 1C.
- Sales owns customers and sales counterparties, pricing policy, and catalog reads for sales.
- Identity owns ASP.NET Identity, roles, authorization policies, session/current-user integration, and seeders.
- BuildingBlocks contains shared abstractions and cross-cutting implementations; do not add business rules there.

## Dependency direction

~~~text
Host.Api / Host.Jobs
        │
        ├── Module.Api ──► Module.Application ──► Module.Domain
        └── Module.Infrastructure ─────────────► Module.Application + Module.Domain

BuildingBlocks.* provides shared primitives and does not depend on business modules.
~~~

The domain layer does not directly depend on application or infrastructure. The application layer does not depend on a concrete `DbContext`, HTTP client, scheduler, or EF repository.
