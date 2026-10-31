# Module rules

When to create a business module, how it is built, where its data lives, and how its input is validated. General layer rules are in the [backend rules](backend-rules.md).

## Owning module first

Every feature belongs to one business module. Before an SDD design, name the owning module from the [module map](../architecture/overview/modules.md) and the data it owns.

- If the feature changes data or rules that an existing module owns, it goes into that module, even when it is large.
- If the feature introduces a new business capability whose data no existing module owns and whose rules do not depend on another module's internals (for example articles and static site pages next to a catalog), propose a new module in the SDD instead of growing an unrelated one. Example: content pages belong in a `Content` module, not in `Catalog`.
- A new module is an architecture decision: record it in the SDD design with the owned data, the public contract other modules may use, and the alternatives considered (extend an existing module or create a new one). The user accepts it before implementation.
- Do not split a capability into a separate module only because it has many files; size is a reason to split folders inside the module.

## Module shape

A new module follows the same clean architecture as the existing ones:

| Project | Owns | References |
| --- | --- | --- |
| `<Module>.Domain` | Aggregates, entities, value objects, domain errors and events, invariants | `BuildingBlocks.Domain` only |
| `<Module>.Application` | Commands, queries, handlers, FluentValidation validators, abstractions for persistence and integrations | `<Module>.Domain`, `BuildingBlocks.Application`, other modules' `Contracts` only |
| `<Module>.Infrastructure` | `DbContext`, entity configurations, migrations, repositories, external adapters, DI registration | `<Module>.Application`, `BuildingBlocks.Infrastructure` |
| `<Module>.Api` | Thin controllers that send one Mediator request and map the `Result` | `<Module>.Application`, `BuildingBlocks.Api` |
| `<Module>.Contracts` (when another module or client consumes it) | Stable request/response DTOs and integration contracts | nothing module-internal |

- Scaffold projects with the .NET CLI (`dotnet new classlib`, `dotnet sln add`, `dotnet add reference`) and match the existing modules' folder layout and project settings instead of writing project files by hand.
- Register the module in the host through one `Add<Module>…` extension per layer, as the existing modules do.
- Add the module to the architecture tests so its project references and isolation are checked.

## Database schema per module

- Each module owns its tables in its own PostgreSQL schema, named after the module in lower case (`content`, `crm`). Its `DbContext` calls `modelBuilder.HasDefaultSchema("<module>")`.
- Keep the module's migration history in its schema: `UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "<module>"))`, in both runtime registration and the design-time factory.
- One module's `DbContext` maps only its own tables. Do not add foreign keys, joins, or EF navigation to another module's tables; store the other module's identifier as a value and read its data through that module's `Contracts` or a published read abstraction.
- The first migration of a new module creates its schema; check it against an empty database as the [database rules](database-rules.md) require.

## Validation

- Every command and every query that accepts client input has a FluentValidation validator (`<UseCase>Validator`) in the use-case folder. `ValidationBehavior` runs it before the handler and turns failures into `Result.Invalid`.
- Validators check the shape of the input: required fields, lengths, ranges, formats, and paging limits. Business invariants and state checks stay in Domain or the handler; do not query the database from a validator unless the project documents an exception.
- Do not repeat validator rules in controllers or request models.

## KedrStore specifics

- Business modules: `Catalog`, `Sales`, `Identity`. Projects live in `src/<Module>/<Module>.<Layer>/`.
- Hosts register modules in `src/Bootstrapper/Host.Api/DependencyInjection/ServiceRegistration/ModulesRegistrationExtensions.cs` and, for jobs, `src/Bootstrapper/Host.Jobs/Host.Jobs/JobsHostServicesExtensions.cs`.
- Architecture tests list business modules in `BusinessModules` in `tests/ArchitectureTests/Modules/ProjectModuleIsolationTests.cs`; add the new module there.
- All modules share one database and run migrations through `Host.Jobs --job=migrate` ([operations](../operations/data/migrations-and-seeding.md)).

## Known deviations

Existing code that does not follow these rules yet. Do not copy it into new modules; change it only as a separate decision.

| Where | Deviation | Plan |
| --- | --- | --- |
| `Catalog`, `Sales`, `Identity` | Tables and migration history in the `public` schema | Keep: moving existing tables needs a separate migration and rollout plan. New modules use their own schema. |
| 30 of 54 commands and queries | No FluentValidation validator | Next backend audit decides which accept client input and need one |
