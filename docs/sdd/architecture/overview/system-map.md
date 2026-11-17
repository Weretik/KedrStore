# System map

The first page to open for "where does this live". It lists every host and every module with one line of what it owns, then points to the context map: who uses whom and how. [Module map](modules.md) gives each module's projects and what its `Contracts` offer; the [feature map](../../features/README.md) goes from a module to its features and code. `tools/docs/check_module_maps.py` runs in CI and fails when a module, host, endpoint folder or feature card is missing from these maps, so a change that adds or renames one updates them in the same pull request ([delivery rules](../../standards/delivery-rules.md)).

## Executable hosts

- `src/Bootstrapper/Host.Api` composes DI, middleware, the modules' Minimal API endpoints, authentication, and OpenAPI.
- `src/Bootstrapper/Host.Jobs` runs one explicitly selected `--job=...` command and exits. The scheduler tick starts it as the Cloud Run Job `kedr-tasks` (`--job=tasks`); it has no persistent internal scheduler. The `--job=migrate` command runs migrations and seeders.

## Business modules

- Catalog owns categories, products, translations, prices, attributes, site models, the product-list read table, and catalog synchronization with 1C.
- Sales owns customers and sales counterparties, pricing policy, manager and site orders, the order workspace, and sending orders to 1C and Telegram.
- Content owns the site texts written in the admin: articles, contacts, requisites, site documents, gallery, home blocks, page search texts, and SEO settings.
- Media owns the picture library on the CDN: folders, file records, uploads, and the CDN sync job.
- Work owns the staff members' own tasks and the automatic log of their admin changes.
- Scheduling owns the background task schedules, the scheduler tick, the outbox run, and the run journal.
- Identity owns ASP.NET Identity, roles, authorization policies, session/current-user integration, and seeders.
- BuildingBlocks contains shared abstractions and cross-cutting implementations; do not add business rules there.

## Context map

The context map is the [C4 component diagram](../c4/README.md#level-3-components-of-the-backend): every module and outside system, and on every arrow how they talk. A `Contracts` interface is a synchronous call inside one process; the outbox is a message sent after the commit and repeated until it succeeds; an adapter in `Infrastructure/Integrations` translates an outside system into the module's own model (anti-corruption layer). No two modules reference each other's `Contracts`, directly or around a circle; `ModuleContractsTests` fails on a cycle.

## Dependency direction

~~~text
Host.Api / Host.Jobs
        │
        ├── Module.Api ──► Module.Application ──► Module.Domain
        └── Module.Infrastructure ─────────────► Module.Application + Module.Domain

BuildingBlocks.* provides shared primitives and does not depend on business modules.
~~~

The domain layer does not directly depend on application or infrastructure. The application layer does not depend on a concrete `DbContext`, HTTP client, scheduler, or EF repository.
