# ADR-0004: Modules talk only through Contracts; folders follow the business

- Status: accepted
- Date: 2026-11-14

## Context

ADR-0001 said a module may use another only through a small `*.Contracts` project, but nothing checked it, and only Catalog and Media had one. A code audit on 2026-11-14 found:

- `Sales.Infrastructure` read Catalog tables through Catalog's `DbContext` interface and used Catalog Application and Domain types.
- Every module's Api referenced `Identity.Application` (and Catalog.Api `Identity.Domain`) for policy and role names.
- Folders inside layers were grouped by kind of class (`Entities/`, `Enums/`, `Errors/`, `ValueObjects/`, `Configurations/`, `Common/`), features had no `Commands/` / `Queries/` split, and the layer documents prescribed that layout.

## Alternatives considered

- Keep ports in the consumer and adapters in the host (the BreviERP ADR-0002 approach): rejected; the host grows into a place that knows every module's data.
- Folders by kind of class: rejected; one feature is spread over many folders and the folders grow without limit.

## Decision

- Every module has five projects: Api, Application, Contracts, Domain, Infrastructure. Another module may reference only `<Module>.Contracts`. The provider implements its contracts and registers them; the host has no cross-module code.
- The host registers each module with one `Add<Module>Module()` call from the module's Api.
- Folders follow the business: Domain by aggregate, Application `Features/<Feature>/Commands|Queries/<UseCase>/`, Infrastructure `Persistence/<Aggregate>/` and `Integrations/<System>/`, Api by feature, tests mirror the code.
- Architecture tests check references, Contracts-only access, folder names, and host content.

Full rules: [module rules](../../standards/module-rules.md). The same rules apply to BreviERP and the shared backend documentation template.

## Consequences

- The existing code is restructured in stages, one module per change; namespaces change with folders, behavior and database do not.
- A cross-module need first adds an interface to the provider's `Contracts`, which is a little more code than reading another table directly.
