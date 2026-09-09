# Test architecture design

## Project responsibilities

`UnitTests` proves deterministic Domain and Application behavior without a
database provider, HTTP host, network, or bootstrapper. It may reference
module Domain and Application assemblies.

`IntegrationTests` proves EF Core mappings and persistence, migrations,
PostgreSQL behavior, external adapter mappings, job orchestration, and HTTP or
OpenAPI behavior. Fast component tests may use EF Core InMemory only when their
risk is service orchestration rather than relational or provider behavior.
Every PostgreSQL-specific claim is covered separately by a PostgreSQL fixture.

`ArchitectureTests` reads the repository project graph and compiled assemblies.
Project-graph tests catch forbidden references even when no type currently uses
them. Assembly tests catch forbidden type dependencies and keep failures close
to the violating type.

## Target layout

```text
tests/
├── UnitTests/
│   ├── BuildingBlocks/<layer>/
│   ├── Catalog/<layer>/
│   ├── Identity/<layer>/
│   └── Sales/<layer>/
├── IntegrationTests/
│   ├── Catalog/{Persistence,OneC,Api}/
│   ├── Identity/Persistence/
│   ├── Sales/{Persistence,OneC,Api}/
│   ├── Platform/Jobs/
│   └── TestSupport/{Database,Sales}/
└── ArchitectureTests/
    ├── Layers/
    ├── Modules/
    └── TestSupport/
```

Only directories with actual tests are created. A module does not receive an
empty folder merely to make the tree symmetrical.

## Sales order-sync test split

The former `SyncOneCOrdersJobTests` responsibilities are split into delivery,
scheduling, recovery, notification, and concurrency classes. Scenario setup,
database construction, fakes, and clocks are shared through Sales test support.
The simulated concurrency interceptor is removed after the real PostgreSQL race
test exists.

## PostgreSQL lifecycle

One xUnit collection fixture owns a `PostgreSqlContainer`. It starts once per
collection, builds isolated databases or resets schema as required, and applies
Sales migrations before tests. Each racing worker creates its own
`SalesDbContext`, so EF change tracking cannot serialize the test accidentally.
The fixture converts a missing Docker endpoint into a descriptive xUnit skip;
other startup or migration failures remain test failures.

## Architecture policy

The allowed production reference direction is:

```text
Host -> Api and Infrastructure -> Application -> Domain
Contracts may be consumed explicitly across a module boundary.
BuildingBlocks layers may be consumed by their corresponding or outer layers.
```

The initial architecture suite encodes rules that match the current documented
architecture. Existing documented cross-module adapters in Infrastructure are
allowed explicitly. New direct Domain/Application/API coupling between Catalog,
Sales, and Identity is rejected unless the architecture documentation and rule
set are deliberately changed together.

BuildingBlocks projects cannot reference a business module. During
implementation, the existing XML-to-JSON adapter and its registration moved
from `BuildingBlocks.Infrastructure` to `Catalog.Infrastructure` so this rule
could be enforced without an exception.

## CI and coverage

The workflow uses a verification job on `ubuntu-latest`. It restores once,
builds once, runs unit and architecture tests, then integration tests with
`XPlat Code Coverage`. GitHub-hosted Linux runners provide Docker for
Testcontainers. The deploy job declares `needs: verify`, so authentication,
image publishing, migrations, and deployment cannot start after a failed gate.
Cobertura output is uploaded even when a later test step fails, where possible.
