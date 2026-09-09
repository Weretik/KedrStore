# Testing rules

## Behavior-first test selection

Every new or changed observable behavior must reference an acceptance scenario
such as `SC-001`. Select the narrowest test level that proves the risk:

- Domain rules and state transitions: Domain unit tests;
- use-case orchestration, validation, and results: Application tests;
- persistence constraints, transactions, and adapters: integration tests;
- HTTP status, serialization, authorization, and OpenAPI behavior: API
  integration or contract tests;
- layer or dependency changes: architecture tests.

Do not repeat the complete scenario at every level without a distinct risk.
One scenario may be supported by several focused tests.

## Test project ownership

- `UnitTests` contains deterministic Domain and Application tests and does not
  reference Infrastructure, Bootstrapper, HTTP hosts, or EF Core InMemory.
- `IntegrationTests` contains persistence, migration, provider, external
  adapter, background-job, HTTP, and OpenAPI tests.
- `ArchitectureTests` verifies project-reference direction, compiled type
  dependencies, business-module isolation, and test source organization.

Within each project, organize tests as `<Module>/<Responsibility>` and use the
matching namespace. Put reusable fixtures, scenario builders, and fakes under
`TestSupport`; do not create empty module directories.

## Relational and provider-specific behavior

Use PostgreSQL integration tests for migrations, SQL, transactions,
constraints, indexes, and PostgreSQL-specific concurrency such as `xmin`.
EF Core InMemory may support fast orchestration tests only when relational and
provider behavior is outside the risk being proved.

PostgreSQL integration tests use Testcontainers and separate `DbContext`
instances for concurrent workers. A missing local Docker endpoint may produce
an explicit skipped test. On CI, where Docker is a prerequisite, container
startup and migration failures fail the verification gate.

## Red -> Green -> Refactor

For new behavior:

1. **Red**: add the smallest test that expresses the selected rule or scenario,
   run it, and confirm that it fails for the expected missing behavior.
2. **Green**: implement the smallest complete behavior that makes the test pass
   without implementing unrelated scenarios.
3. **Refactor**: improve names and structure while preserving architectural
   boundaries, then rerun the focused tests.
4. **Regression**: run the affected test set and record the command and result.

A task is not complete until its evidence names the test and records the Red,
Green, and regression results. Do not treat compilation errors, broken test
setup, or unrelated failures as a valid Red result.

## Exceptions

A Red-first test may be omitted for documentation-only work, generated
migrations, exploratory spikes, or infrastructure changes that cannot be
observed meaningfully before a prerequisite exists. Record the reason, the
replacement verification, and the scenarios enabled or covered. An exception
is not permission to omit final verification.

## Repository verification

Before completing a feature, run where applicable:

```powershell
dotnet restore KedrStore.sln
dotnet build KedrStore.sln --no-restore
dotnet test KedrStore.sln --no-build
```

If verification is blocked or fails, state the exact command, failure point,
and whether the failure is pre-existing or caused by the change. Do not hide
unrelated failures.
