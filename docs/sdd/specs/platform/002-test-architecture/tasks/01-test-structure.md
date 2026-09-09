# TS-001 — Reorganize tests and correct project ownership

- **Covers:** SC-001, SC-002
- **Depends on:** none
- **Exact paths:** `tests/UnitTests`, `tests/IntegrationTests`
- **Test level:** unit and integration regression

## Work

- [x] Move tests into module and responsibility directories and matching namespaces.
- [x] Split the Sales order-sync job suite into focused classes with shared support.
- [x] Move infrastructure and host tests out of `UnitTests`.
- [x] Remove EF Core InMemory, Infrastructure, and Host project references from `UnitTests`.
- [x] Delete `UnitTest1.cs`.

## Test-first exception and evidence

- Reason: this task preserves existing behavior while changing ownership and structure.
- Before baseline: `dotnet test KedrStore.sln --no-restore --verbosity minimal` passed 65 unit and 22 integration tests on 2026-09-09; ArchitectureTests did not execute.
- After regression: `dotnet test tests/UnitTests/UnitTests.csproj --no-build --configuration Release` passed 41 tests; `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-build --configuration Release` passed 44 and skipped the Docker-dependent PostgreSQL test.

## Checkpoint

All existing tests compile and pass from their target project, and UnitTests has
no outward production-layer dependency.
