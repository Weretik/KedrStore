# Test architecture — scope and acceptance scenarios

## Scope

The repository needs a test layout that remains understandable as Catalog,
Sales, Identity, BuildingBlocks, and host behavior grow. Test ownership must be
visible from paths and namespaces, database-specific risks must run against
PostgreSQL, architectural boundaries must fail the build when violated, and
deployment must depend on successful verification.

Included:

- module and layer folders in every test project;
- focused test classes and reusable test support;
- migration of infrastructure and job tests out of `UnitTests`;
- PostgreSQL integration fixtures based on Testcontainers;
- a real concurrent `xmin` claim test using separate EF Core contexts;
- executable architecture tests for project and assembly dependencies;
- CI restore, build, tests, coverage collection, and artifact publication
  before Cloud Run deployment;
- deletion of the placeholder test.

Excluded:

- production behavior changes;
- browser end-to-end tests;
- replacement of xUnit or hand-written fakes;
- a repository-wide coverage threshold until a measured baseline is reviewed.

## Rules

- **R-001:** Unit tests reference only the production assemblies needed for
  domain and application behavior; infrastructure and host behavior belongs in
  integration tests.
- **R-002:** Test paths and namespaces identify the owning module and tested
  layer or adapter category.
- **R-003:** Provider-specific persistence and concurrency behavior is proved
  against a temporary PostgreSQL database with migrations applied.
- **R-004:** Architecture tests enforce the documented layer direction and
  prohibit direct dependencies between business modules except through an
  explicitly allowed contract or application abstraction.
- **R-005:** Production deployment starts only after restore, build, unit,
  integration, and architecture tests succeed.
- **R-006:** CI collects Cobertura coverage and publishes it as a build
  artifact; coverage is observable without inventing an unreviewed threshold.
- **R-007:** Testcontainers-dependent tests report an explicit skip when Docker
  is unavailable locally and execute normally on Docker-enabled CI runners.

## Acceptance scenarios

### SC-001 — Test ownership is visible

**Given** the repository test projects, **when** a contributor locates a test,
**then** its path and namespace identify its module and responsibility, shared
setup is under `TestSupport`, and no placeholder test remains.

### SC-002 — Unit tests stay inside inner layers

**Given** the `UnitTests` project, **when** its project references are inspected,
**then** it does not reference Infrastructure, Bootstrapper, or EF Core
InMemory, while domain and application tests continue to pass.

### SC-003 — PostgreSQL proves order-sync concurrency

**Given** one due Sales order synchronization row and two independently scoped
workers, **when** both workers race to claim it, **then** PostgreSQL `xmin`
optimistic concurrency permits only one 1C send and persists one accepted
state.

### SC-004 — Architecture drift fails tests

**Given** the documented solution boundaries, **when** project references and
compiled type dependencies are checked, **then** forbidden inward-to-outward
layer dependencies and forbidden business-module dependencies fail with the
offending projects or types named.

### SC-005 — Verification gates deployment

**Given** a push to `master`, **when** the Cloud Run workflow executes, **then**
deployment waits for restore, build, all test projects, PostgreSQL tests, and
coverage collection, and the coverage artifact is retained for inspection.
