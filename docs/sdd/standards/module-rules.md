# Module rules

When to create a business module, how it is built, how modules talk to each other, how folders are organized inside it and in tests, where its data lives, and how its input is validated. The same text is used in BreviERP and in the shared backend documentation template. General layer rules are in the [backend rules](backend-rules.md).

## Owning module first

Every feature belongs to one business module. Before an SDD design, name the owning module from the [module map](../architecture/overview/modules.md) and the data it owns.

- If the feature changes data or rules that an existing module owns, it goes into that module, even when it is large.
- If the feature introduces a new business capability whose data no existing module owns and whose rules do not depend on another module's internals (for example articles and static site pages next to a catalog), propose a new module in the SDD instead of growing an unrelated one. Example: content pages belong in a `Content` module, not in `Catalog`.
- A new module is an architecture decision: record it in the SDD design with the owned data, the public contract other modules may use, and the alternatives considered (extend an existing module or create a new one). The user accepts it before implementation.
- Do not split a capability into a separate module only because it has many files; size is a reason to split folders inside the module.

## Solution root

```text
KedrStore.sln
├── Directory.Build.props      settings shared by every project: TargetFramework, Nullable, ImplicitUsings, analyzers
├── Directory.Packages.props   central package management: every NuGet version, ManagePackageVersionsCentrally=true
├── global.json                SDK version
├── src/                       Bootstrapper/, BuildingBlocks/, <Module>/
├── tests/                     UnitTests/, IntegrationTests/, ArchitectureTests/
├── tools/                     scripts the team runs by hand
└── docs/
```

- A `.csproj` holds only what is specific to that project: its SDK, references, and `<PackageReference Include="..." />` without `Version`. Settings that every project shares go into `Directory.Build.props`; package versions go only into `Directory.Packages.props`, so one package has one version in the whole solution.
- Dockerfiles that restore before copying the sources copy `Directory.Build.props`, `Directory.Packages.props`, and `global.json` together with the project files.
- No other files at the root besides the solution, these props files, repository configuration (`.editorconfig`, `.gitignore`, `.gitattributes`, `.config/`), `README.md`, `AGENTS.md`, `CLAUDE.md`, and the license.

## Module shape

Every business module has the same five projects. The host only registers modules; it holds no business code.

| Project | Owns | References |
| --- | --- | --- |
| `<Module>.Domain` | Aggregates, entities, value objects, domain errors and events, invariants | `BuildingBlocks.Domain` only |
| `<Module>.Application` | Commands, queries, handlers, FluentValidation validators, ports for persistence and integrations | `<Module>.Domain`, `<Module>.Contracts`, `BuildingBlocks.Application`, other modules' `Contracts` only |
| `<Module>.Infrastructure` | `DbContext`, entity configurations, migrations, repositories, external clients, implementations of the module's own `Contracts` | `<Module>.Application`, `<Module>.Domain`, `BuildingBlocks.Infrastructure`, other modules' `Contracts` only |
| `<Module>.Api` | Thin controllers that send one Mediator request and map the `Result`; the `Add<Module>Module()` registration | `<Module>.Application`, `<Module>.Infrastructure` (registration only), `BuildingBlocks.Api`, other modules' `Contracts` only (for example access policy names) |
| `<Module>.Contracts` | What other modules may use: read and command interfaces, their DTOs, integration events, shared constants | no project of this or another module; only base packages such as `Ardalis.Result` |
| Host | `Program.cs`, configuration, middleware, the list of modules | each module's `Api` (and `Infrastructure` for job or seed hosts) |

- Scaffold projects with the .NET CLI (`dotnet new classlib`, `dotnet sln add`, `dotnet add reference`) and match the existing modules' folder layout and project settings instead of writing project files by hand.
- Create `<Module>.Contracts` together with the module, even while it is empty: it is the only place another module may look at.
- Register the module in the host with one call, `Add<Module>Module(configuration)`, defined in `<Module>.Api/DependencyInjection/`. It registers the module's Application, Infrastructure, Contracts implementations, and controllers. Controllers and request models never use Infrastructure types.
- **Ports and DI, one chain.** Application declares what it needs as interfaces in `Application/Abstractions/` (`IOrderRepository`, `IUnitOfWork`, `IOneCClient`; for time use .NET `TimeProvider`). Infrastructure implements them (`Persistence/` for the `DbContext` and repositories, `Integrations/<System>/` for a client and its options) and binds them in one public entry point, `Add<Module>Infrastructure(configuration)`, in `Infrastructure/DependencyInjection/`. `Add<Module>Module(configuration)` in the Api calls it plus the Application registration (validators) and controllers. The host's `Program.cs` calls only `Add<Module>Module(...)` for each module. Smaller public `Add*` helpers exist only when a job host or a test needs a subset, and say so in their summary.
- **Lifetimes.** A Singleton (and any `BackgroundService` or `IHostedService`, which are Singletons) never takes a Scoped service such as a `DbContext`, a repository, or the current user in its constructor; it takes `IServiceScopeFactory` and creates a scope per operation (`using var scope = scopeFactory.CreateScope();`). Both hosts turn on `ValidateScopes` and `ValidateOnBuild` in every environment, so a captive dependency or a missing registration fails at startup; `ServiceProviderValidationTests` builds both containers in Production mode.
- Add the module to the architecture tests so its project references, its isolation, and its folder rules are checked.

## Shared building blocks

Code that every module uses lives in `src/BuildingBlocks/` (some solutions call these projects `Shared.*`; the name does not matter, the content rules do). Each shared project mirrors one layer and follows that layer's dependency rules.

```text
src/BuildingBlocks/
├── BuildingBlocks.Domain           kernel: Entity, AggregateRoot, IDomainEvent, Result/Error, Money, strongly typed ids, Guard
├── BuildingBlocks.Application      Mediator pipeline behaviors, ports (ICurrentUser, IClock, ICacheService, INotificationSender), paging, text and phone normalization
├── BuildingBlocks.Infrastructure   EF base DbContext and interceptors, domain-event dispatch, outbox, logging, caching, notification and other client implementations, migration and seed runners
└── BuildingBlocks.Api              Result→HTTP mapping, problem details, exception and correlation middleware, endpoint conventions
```

- `BuildingBlocks.Domain` (the kernel) holds only primitives that have no business meaning of their own: base entity and event types, `Result`/`Error`, `Money`, ids, guards. No EF Core, no ASP.NET Core or HTTP, no logging, no configuration, no business rule of any module. Allowed packages: value-object and result libraries (for example `Vogen`, `Ardalis.Result`, `Ardalis.SmartEnum`, a money type).
- `BuildingBlocks.Application` holds use-case plumbing and ports: pipeline behaviors and the interfaces that Infrastructure implements. It references no EF Core, database driver, ASP.NET Core Identity, HTTP client, bot, file-format, or scheduler package; their implementations go to `BuildingBlocks.Infrastructure` (or a module's Infrastructure) behind a port.
- `BuildingBlocks.Infrastructure` holds the technical implementations every module shares: base persistence, the outbox when the project has one, logging and telemetry setup, cache, notification clients, middleware that is not HTTP-specific. It references `BuildingBlocks.Application` and `BuildingBlocks.Domain`, never a module.
- `BuildingBlocks.Api` holds HTTP-only helpers and middleware. It references `BuildingBlocks.Application`, never a module.
- An integration with one external system that several modules use is its own project, `BuildingBlocks.Integrations.<System>`, with the Infrastructure rules.
- No business logic in building blocks: a rule that belongs to one module (pricing, order status, catalogue codes) stays in that module, even when two modules need it; the other module reads it through `Contracts`.
- Folders inside a shared project follow the same rules as a module: by concern (`Persistence/`, `Outbox/`, `Logging/`, `Notifications/`, `Behaviors/`, `Results/`), never `Helpers/`, `Utils/`, `Extensions/`, `Services/`, `Common/`, `Contracts/`, or a misspelled name. Registration extensions go to `DependencyInjection/`.
- The architecture tests check the package and project references of each building-block project and that no building-block project references a module.

## Transactional outbox

A use case that writes data and also has an external side effect (1C or another ERP, Telegram or e-mail, site revalidation, payment or delivery API, CDN upload) or must tell another module about the change does not call the outside world inside or around its transaction. It writes an outbox row in the same `SaveChanges`; after the commit the request tries the row once, and a dispatcher retries what is left.

- The outbox table and the dispatcher are shared code in `BuildingBlocks.Infrastructure`; each module owns its rows (in its own schema) and its handlers. Handlers are idempotent: the receiver gets a stable key and ignores a repeat.
- On a scale-to-zero host such as Cloud Run there is no in-process loop, hosted timer, or Hangfire. One protected internal endpoint per backend (`POST /internal/outbox/dispatch`, OIDC token of the scheduler's service account) is called every minute by one Cloud Scheduler job; it returns at once when nothing is due. No message bus or MassTransit.
- A failed row stays visible (admin page or log alert) with its last error after the retry limit; nothing is dropped silently.
- Not for work inside one module's transaction, for read-only calls, or when the user must see the external result synchronously.
- The SDD design names the message types, payload, handler, idempotency key, and retry limit. Details: the `backend-feature-spec` skill reference `references/transactional-outbox.md`.

**KedrStore today:** there is no shared outbox yet (a planned stage of the architecture restructure). `Sales` already keeps a module-specific outbox for orders sent to 1C (`OneCOrderSync` rows with status, retries, and a dead-letter Telegram alert), processed by a `Host.Jobs` job. The public quick order (`Catalog` `CreateOrder`) sends its Excel file to Telegram without saving the order; when site orders start being saved, that send goes through the outbox.

## Communication between modules

A module never reads another module's database, entities, `DbContext`, repositories, handlers, or Domain types. It uses only the other module's `Contracts` project.

- **Provider.** The module that owns the data publishes what others need in `<Module>.Contracts`: a small interface such as `IProductPriceReader`, the DTOs it returns (plain records, not Domain types), and integration events. The provider implements the interface in its Infrastructure (or Application) and registers it in `Add<Module>Module()`.
- **Consumer.** The module that needs the data references the provider's `Contracts` project from its Application (or Infrastructure, for an adapter) and injects the interface. Its unit tests replace the interface with a fake.
- **Reads** go through a contract interface. **"After X happens"** reactions go through an integration event declared in the provider's `Contracts` and handled in the consumer's Application, so the provider does not know the consumer.
- **Commands** to another module, such as "create a production order", are also a contract interface or an event; never call another module's handler or Mediator request type directly.
- Keep each contract small and named in the provider's words. Change it compatibly: add members or new interfaces instead of changing a shape a consumer already uses.
- The host does not implement cross-module adapters. An adapter that maps a provider's contract into the consumer's own port lives in the consumer's Infrastructure under `Adapters/<Provider>/`.

## Folder structure inside a module

Folders follow the business, not the kind of class. The same applies to the whole repository: no dump folders such as `Common/`, `Misc/`, `Helpers/`, `Utils/`, `Temp/`, or `Old/`, and no stray scripts or files at a project root. Do not create layer-wide or per-aggregate type folders such as `Entities/`, `Enums/`, `Errors/`, `ValueObjects/`, `Interfaces/`, `DTOs/`, `Models/`, `Services/`, or `Configurations/` that collect unrelated features. Namespaces follow folders.

```text
<Module>.Domain/
  <Aggregate>/                     Order.cs, OrderLine.cs, OrderStatus.cs, OrderId.cs, OrderErrors.cs, OrderPlaced.cs
  <Aggregate>/<Part>/              only when the aggregate grows: a sub-concept such as Lines/, not a type
<Module>.Application/
  Abstractions/                    ports: what Application asks from the outside world (IOrderRepository, IUnitOfWork, IClock, ICatalogDbContext)
  Features/<Feature>/
    Commands/<UseCase>/            PlaceOrderCommand.cs, PlaceOrderHandler.cs, PlaceOrderValidator.cs, PlaceOrderResponse.cs
    Queries/<UseCase>/             GetOrderDetailsQuery.cs, GetOrderDetailsHandler.cs, GetOrderDetailsValidator.cs, OrderDetailsDto.cs
    EventHandlers/                 handlers of domain or other modules' integration events for this feature
    Shared/                        DTOs, mapping, and policies used by several use cases of this feature
<Module>.Infrastructure/
  Persistence/                     <Module>DbContext.cs, design-time factory, Migrations/
  Persistence/<Aggregate>/         entity configurations, repository, read queries of that aggregate
  Integrations/<System>/           external clients such as 1C, Telegram, storage
  Adapters/<Provider>/             adapters from another module's Contracts to this module's own ports
  Contracts/<Feature>/             implementations of the module's own Contracts interfaces
  DependencyInjection/             registration of the layer
<Module>.Api/
  Features/<Feature>/              controllers of the feature (admin, public, mobile) and their request models
  DependencyInjection/             Add<Module>Module()
<Module>.Contracts/
  <Feature>/                       interfaces, DTOs, integration events, constants other modules use
```

- **Domain.** One folder per aggregate holds its root, child entities, value objects, enums, errors, and events. A value object used by several aggregates gets a folder named after the concept (`Money/`, `Slugs/`).
- **Application, one use case = one folder.** Every use case is one folder under `Commands/` (changes state) or `Queries/` (reads only), named after the action (`PlaceOrder`, `GetOrderDetails`). The folder holds, flat and together, everything of that use case: the request (`<UseCase>Command` or `<UseCase>Query`), its handler (`<UseCase>Handler`), its validator (`<UseCase>Validator`), and its result (`<UseCase>Response` for a command, `<Name>Dto` for a query). One request per folder: a second query or command is a second folder. No subfolders inside a use case (`DTOs/`, `Validators/`, `Models/`); a type that two use cases share goes to the feature's `Shared/`. Ports that Application needs from the outside world (repositories, unit of work, clock, read `DbContext` interfaces, external services) live in `Abstractions/` and are implemented in Infrastructure. A command handler never returns a read model bigger than the new identifier or status; a query handler never changes state. Admin, public, and mobile variants of the same feature live in the same feature folder as separate use cases (`GetRedirects`, `GetPublicRedirects`), not in separate `Public…` feature folders. A use case's request and response types live in its folder; the name `Contracts` is reserved for the module's Contracts project.
- **Infrastructure.** Persistence is grouped by aggregate, the same names as in Domain; integrations by external system.
- **Api.** Controllers are grouped by the same features as Application. One controller serves one audience (admin, public, mobile) of one feature.
- A folder with eight or more production files is a signal to look for a sub-concept to split out, not a reason to create type folders.

## Layer responsibilities and the handler

| Where | Does | Example |
| --- | --- | --- |
| Handler (Application) | the order of steps | load, call the domain, save |
| Aggregate (Domain) | the rules | no zero quantity, minimum order sum 500, a shipped order cannot be cancelled |
| Infrastructure | how exactly data is stored and fetched | EF Core, the 1C HTTP client |
| Api | HTTP | route, request model, status codes |

A command handler follows five steps:

1. Load the data it needs (aggregates through the module's ports, read models through `Contracts` of other modules).
2. Check what needs outside data: the referenced product exists, a slug is free, a configured root category is excluded. Return `NotFound` / `Invalid` / `Conflict` here.
3. Call the aggregate's method; the business rules are checked there and raise a domain error. The handler does not repeat or replace them (no `if (order.Status == Shipped) return Conflict` next to `order.Cancel()`); the pipeline maps a domain error to the response.
4. Save everything with one `SaveChanges`, that is one transaction. A batch command that processes items independently (an import, a migration of old files) may save once per item and says so in its summary.
5. Return a simple result: the new id, a status, or a DTO from the use-case folder, never a domain entity or value object.

- **Rich domain.** Entities have private setters and change state only through methods that check their invariants; a rule that involves only one aggregate's own data lives in that aggregate, not in the handler. A rule that needs several aggregates of the same module goes to a domain service in `Domain/<Aggregate>/`, called by the handler.
- **Queries** read through projections straight to DTOs (`Select`), never load aggregates to map them, and never call `SaveChanges`.
- **Api** controllers send one Mediator request and map the `Result` to HTTP; they hold no rule, no query, and no domain type.
- The architecture tests check that request result types and Api code use no Domain types and that Domain entities have no public setters.

## Test structure

Tests mirror the code they check, so a test is found the same way as its code.

```text
tests/UnitTests/<Module>/Domain/<Aggregate>/                <Aggregate>Tests.cs
tests/UnitTests/<Module>/Application/<Feature>/             <UseCase>HandlerTests.cs, <UseCase>ValidatorTests.cs
tests/IntegrationTests/<Module>/Api/<Feature>/              HTTP tests of the feature's endpoints
tests/IntegrationTests/<Module>/Persistence/<Aggregate>/    database tests of that aggregate
tests/IntegrationTests/<Module>/Integrations/<System>/      external system adapters
tests/IntegrationTests/<Module>/Contracts/<Feature>/        the module's Contracts implementations
tests/IntegrationTests/Platform/                            host, pipeline, jobs
tests/ArchitectureTests/<Rule>/                             one folder per rule group: Layers, Modules, Folders, Hosts
tests/<Kind>/TestSupport/                                   factories, fakes, builders shared by that test project
```

- A test file sits in exactly one module and one layer folder; a namespace follows its path. No loose files at a test project root and no folders such as `Misc/`, `Helpers/`, `Common/`, or `Temp/`.
- Every module has at least: unit tests of its Domain aggregates, unit tests of its command and query handlers and validators, integration tests of its endpoints and persistence, and its place in the architecture tests.

## Architecture tests

The rules above are only real when a test fails on a violation. The architecture test project checks, for every business module:

- project references: Domain only `BuildingBlocks.Domain`; Application, Infrastructure, and Api reference another module only through its `Contracts`; `Contracts` references no module project;
- code dependencies (namespaces), the same rules, so a transitive reference cannot bypass them;
- Api controllers do not use Infrastructure types;
- folders: no type folders (`Entities`, `Enums`, `Errors`, `ValueObjects`, `Configurations`, `Interfaces`, `DTOs`, `Models`, `Common`, `Helpers`, `Utils`), command and query handlers sit under `Features.<Feature>.Commands.<UseCase>` / `.Queries.<UseCase>` with no deeper folder, each use-case namespace holds one request and one handler, ports sit in `Abstractions`;
- hosts contain no class that implements a module port;
- test sources sit in a responsibility folder and their namespace follows the path.

A new module, layer, or rule is added to these tests in the same change, test first: the new architecture test must fail on the old code (Red) before the code is moved (Green), as the project's testing rules require for any behavior.

## Hosts and jobs

A host is the composition root, nothing more.

```text
Host.Api/
  Program.cs                       pipeline and startup
  appsettings*.json, Dockerfile
  DependencyInjection/             one file per technical concern: modules list, security, web, observability, options
```

- The host calls `Add<Module>Module()` for each module and configures cross-cutting concerns (authentication, CORS, logging, health checks, OpenAPI). It does not contain readers, adapters, notifiers, or any class that knows two modules' data; such a class belongs to a module's Infrastructure behind a `Contracts` interface.
- Access policies are declared once in the host from the names in `Identity.Contracts` (or the project's access module).
- A job or seed host (`Host.Jobs`, `Host.Seed`) only parses the command line, picks the job, and runs it. The job's logic is a use case of the module that owns the data (a command in its Application, run through Mediator or a published job interface in its `Contracts`), or a separate job project for a large one-off import.
- A job project has its own folders by step (`Source/`, `Planning/`, `Execution/`), references the owning module's `Contracts` (or, for a data import that owns no tables, that module's Infrastructure as a documented exception), and never maps another module's tables in its own `DbContext`.

## Database schema per module

- Each module owns its tables in its own PostgreSQL schema, named after the module in lower case (`content`, `crm`). Its `DbContext` calls `modelBuilder.HasDefaultSchema("<module>")`.
- Keep the module's migration history in its schema: `UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "<module>"))`, in both runtime registration and the design-time factory.
- One module's `DbContext` maps only its own tables. Do not add foreign keys, joins, or EF navigation to another module's tables; store the other module's identifier as a value and read its data through that module's `Contracts`.
- The first migration of a new module creates its schema; check it against an empty database as the [database rules](database-rules.md) require.

## Validation

- Every command and every query that accepts client input has a FluentValidation validator (`<UseCase>Validator`) in the use-case folder. `ValidationBehavior` runs it before the handler and turns failures into `Result.Invalid`.
- Validators check the shape of the input: required fields, lengths, ranges, formats, and paging limits. Business invariants and state checks stay in Domain or the handler; do not query the database from a validator unless the project documents an exception.
- Do not repeat validator rules in controllers or request models.

## KedrStore specifics

- Business modules: `Catalog`, `Sales`, `Identity`, `Content` (site articles; schema `content`), `Media` (CDN picture library; schema `media`), `Work` (staff tasks and activity log; schema `work`). Projects live in `src/<Module>/<Module>.<Layer>/`.
- `Host.Api` calls `Add<Module>Module()` for each module in `src/Bootstrapper/Host.Api/DependencyInjection/ServiceRegistration/ModulesRegistrationExtensions.cs`; each module defines it in `<Module>.Api/DependencyInjection/<Module>Module.cs`. `Host.Jobs` registers module Infrastructure and jobs (`AddCatalogJobs()`) in `src/Bootstrapper/Host.Jobs/Host.Jobs/JobsHostServicesExtensions.cs`.
- Access policy and role names are in `Identity.Contracts/Authorization` (`PolicyNames`, `RoleNames`); every module's Api uses them from there.
- Architecture tests list business modules in `BusinessModules` in `tests/ArchitectureTests/Modules/ProjectModuleIsolationTests.cs`; add the new module there.
- All modules share one database and run migrations through `Host.Jobs --job=migrate` ([operations](../operations/data/migrations-and-seeding.md)).

## Known deviations

Existing code that does not follow these rules yet. Do not copy it into new modules. The restructuring that removes the first rows runs in stages (owner decision 2026-11-14); each stage removes its row.

| Where | Deviation | Plan |
| --- | --- | --- |
| `Sales`, `Work` | No `<Module>.Contracts` project yet (`Content.Contracts` created empty) | Created empty with the folder restructuring |
| Host.Api | One Mediator registration lists every module's Application assembly | Keep: the Mediator source generator needs one `AddMediator` call with literal assemblies |
| All modules | Type folders (`Entities/`, `Enums/`, `Errors/`, `ValueObjects/`, `Configurations/`, `Common/`), Application features without `Commands/` / `Queries/`, controllers in `Controllers/` (Catalog and Content done) | Stage: folder restructuring, one module per change |
| All Application projects | Use cases named `<UseCase>CommandHandler` / `<UseCase>QueryHandler`; `DTOs/` and `Validators/` subfolders inside use cases; several queries in one use-case folder (`Products/GetAdminList`); ports in `Application/Contracts/` instead of `Abstractions/` (Catalog and Content done) | Folder restructuring, one module per change |
| Handlers (audit 2026-11-14 of 96 commands) | Domain is rich (no public setters, rules raise `DomainException`), and no command returns an entity. Legal documents: the publish rules (date not before the current edition, next number, archive the current one) live in `LegalDocumentVersion`; handlers still check `IsDraft` first only to answer 409, because a domain error has no type yet and becomes 400 | Fixed with the folder restructuring of each module |
| Side effects | No shared outbox: Sales uses its own `OneCOrderSync` table and job; the quick order sends Telegram directly | Stage: transactional outbox (Cloud Scheduler calls one internal endpoint every minute) |
| Architecture tests | Check only Domain and Application references; Contracts, folder, and host rules not checked | Stage: architecture tests for all rules |
| Tests | No unit tests for Identity; few handler tests for Content, Media, Work | Stage: missing unit tests |
| Repository root | `dotnet-install.ps1`, `SALES_1C_SYNC_INSTRUCTION.txt` | Move to `tools/` and `docs/sdd/operations/` with the folder restructuring |
| `Catalog.Application`, `Content.Application`, `Media.Application`, `Sales.Application`, `Work.Application` | Reference EF Core directly: handlers query the module's read `DbContext` interface (`DbSet`, `EF.Functions.ILike`); `Catalog.Application` also references import packages (ClosedXML, Newtonsoft.Json, Slugify, NodaMoney, Npgsql) | Folder restructuring moves import and job code to Infrastructure; the read-`DbContext` pattern stays until a separate decision |
| `BuildingBlocks.*` | Type and dump folders (`Application/Contracts/`, `Application/Helpers/`, `Infrastructure/Extensions/`, `Infrastructure/Services/`); no outbox | Folder restructuring renames them by concern; an outbox is added only with a feature that needs reliable cross-module events |
| Identity, Media, Sales, Work Application and Infrastructure | Ports in `Application/Contracts/` instead of `Abstractions/`; Infrastructure registered through several public `Add*Services` methods instead of one `Add<Module>Infrastructure()` (Catalog and Content done) | Folder restructuring, one module per change |
| Errors | Domain errors (`DomainError(Code, Message)`) have no type, so every `DomainException` becomes 400; about 15 controller actions choose status codes themselves instead of the one `ToActionResult` mapper; error bodies are not `ProblemDetails` | Error type and one mapper: stage with the next modules; `ProblemDetails` body: switched together with the Minimal API stage and the client updates (owner decision 2026-11-14) |
| `Catalog`, `Sales`, `Identity` | Tables and migration history in the `public` schema | Keep: moving existing tables needs a separate migration and rollout plan. New modules use their own schema. |
| 58 of 173 commands and queries | No FluentValidation validator | Final audit decides which accept client input and need one |
