# TS-003 — Enforce project and assembly boundaries

- **Covers:** SC-002, SC-004
- **Depends on:** TS-001
- **Exact paths:** `tests/ArchitectureTests`, production `.csproj` files
- **Test level:** architecture

## Work

- [x] Convert `ArchitectureTests` into an xUnit test project.
- [x] Test project-reference direction for Domain, Application, API, Infrastructure, hosts, and test projects.
- [x] Test compiled Domain and Application type dependencies.
- [x] Test business-module isolation with documented explicit allowances.
- [x] Prevent shared BuildingBlocks projects from referencing business modules.
- [x] Include actionable failure messages naming violations.

## TDD evidence

- Red: current project contains no test SDK and `dotnet test` executes no architecture tests.
- Green/refactor: removed the unused `Identity.Api -> Identity.Infrastructure`
  project reference, moved the XML-to-JSON adapter into Catalog to remove
  `BuildingBlocks.Infrastructure -> Catalog.Application`, and refined host classification.
- Regression: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-build --configuration Release` passed 10 tests.

## Checkpoint

ArchitectureTests executes in `dotnet test KedrStore.sln` and all encoded rules
match the documented current architecture.
