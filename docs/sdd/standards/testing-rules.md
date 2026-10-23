# Testing rules

## Selecting tests by behavior

Every new or changed observable behavior must link to an acceptance scenario such as `SC-001`. Choose the narrowest test level that proves the required property:

- Domain rules and state transitions: Domain unit tests;
- use-case coordination, validation, and results: Application tests;
- persistence constraints, transactions, and adapters: integration tests;
- HTTP status codes, serialization, authorization, and OpenAPI behavior: API integration or contract tests;
- layer or dependency changes: architecture tests.

Do not repeat an entire scenario at every level without a separate risk. Several focused tests can support one scenario.

## Test project responsibilities

- `UnitTests` contains deterministic Domain and Application tests and does not reference Infrastructure, Bootstrapper, HTTP hosts, or EF Core InMemory.
- `IntegrationTests` contains persistence, migration, provider, external adapter, background job, HTTP, and OpenAPI tests.
- `ArchitectureTests` checks project reference direction, compiled type dependencies, business-module isolation, and test organization.

In each project, organize tests by `<Module>/<Responsibility>` with a matching namespace. Put shared fixtures, scenario builders, and fakes in `TestSupport`; do not create empty module directories.

## Relational and provider-specific behavior

Use PostgreSQL integration tests for migrations, SQL, transactions, constraints, indexes, and PostgreSQL-specific concurrency such as `xmin`. EF Core InMemory may help with fast orchestration tests only when relational and provider behavior are outside the risk being tested.

PostgreSQL integration tests use Testcontainers and separate `DbContext` instances for concurrent actors. If a local Docker endpoint is unavailable, a test may explicitly skip. In CI, Docker is a prerequisite, so a failure to start the container or apply migrations fails verification.

## Red → Green → Refactor

For new behavior:

1. **Red**: add the smallest test for the selected rule or scenario, run it, and confirm that it fails specifically because the behavior is missing.
2. **Green**: implement the smallest complete behavior that passes the test without unrelated scenarios.
3. **Refactor**: improve names and structure while preserving architecture boundaries, then rerun the focused tests.
4. **Regression**: run the affected test set and record the command and result.

A task is not complete until its evidence names the test and records Red, Green, and regression results. A compilation error, broken test environment, or unrelated failure is not a valid Red result.

## Exceptions

A Red-first test can be omitted for documentation-only work, generated migrations, exploratory spikes, or infrastructure changes that cannot be meaningfully observed before a prerequisite exists. Record the reason, replacement check, and scenarios enabled or covered. The exception does not remove final verification.

## Verification scope during implementation

| When | Minimum action |
| --- | --- |
| Local change | Run `dotnet test tests/<Project>/<Project>.csproj --filter <name>` for the affected behavior; for documentation, check links and run `git diff --check`. |
| Boundary, API, or database change | Add the relevant `ArchitectureTests` or `IntegrationTests` project/filter and the needed contract or operational evidence. |
| Full CI | The `verify` job in the [workflow](../../../.github/workflows/deploy-cloudrun.yml) checks PRs and pushes; rerun the full suite locally only on explicit request. |

Check project names and available filters under `tests/` before running them.

While implementing a feature, run only automated tests for the changed feature and directly related components and contracts. Choose the narrowest available projects, filters, and commands that test the specific risk. Record commands run, results, and unverified risks.

Run a general module, application, or repository test suite, full `restore` and `build`, broad regression, coverage, or a full audit only at the user's explicit request. An unrequested full run is not a completion condition. If a required focused check is blocked or fails, name the exact command, failure point, and known impact on the result; do not hide unrelated errors.
