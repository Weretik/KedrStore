# ADR-0001: Modular monolith with one PostgreSQL schema per module

- Status: accepted
- Date: 2026 (the current module layout; the project started as a classic layered app in May 2025)

## Context

One developer builds and runs the whole platform. The business has several clear areas: catalog and 1C import, customers and orders, content of the site, media on the CDN, staff accounts, staff tasks. They change at different speeds, and some of them (Catalog) are large. A team of one cannot afford the cost of microservices: separate deploys, network calls between services, distributed transactions, more cloud bills.

## Alternatives considered

- Microservices per module: rejected; separate deploys, network calls and distributed data are too costly for one developer.
- A single layered app without module boundaries (the 2025 start): rejected; areas started to depend on each other's tables.

## Decision

- One deployable API (`Host.Api`) and one job runner (`Host.Jobs`), built from the same solution.
- Six business modules: Catalog, Sales, Identity, Content, Media, Work. Each has four layers (Api, Application, Domain, Infrastructure) following Clean Architecture.
- Each module owns its own PostgreSQL schema, its own `DbContext` and its own migration history. No foreign keys and no joins across schemas; a reference to another module's data is stored as an ID value.
- A module may use another module only through a small `*.Contracts` project (for example `Catalog.Contracts`, `Media.Contracts`).
- Architecture tests (xUnit) check layer and module rules on every build.

## Why

- One deploy, one process, simple local run and debugging.
- Clear ownership of data: a schema change in one module cannot break another module's queries.
- A module can later become a service, because its data and its contract are already separate.

## Trade-offs

- Reads that need data from two modules go through a contract call, not a SQL join. Some admin screens make more calls than a single query would.
- Migrations are per module, so a release can contain several migration histories; the migration job runs them in a fixed order.
- One module (Catalog) grew to almost half of the backend code; the boundary rules do not stop a module from growing inside.
