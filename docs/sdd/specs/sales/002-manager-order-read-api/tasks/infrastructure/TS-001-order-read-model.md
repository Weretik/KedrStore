# TS-001 — Implement manager-order read model

- **Task ID:** TS-001
- **Covers:** SC-001, SC-002, SC-003, SC-004
- **Depends on:** EN-002
- **Exact paths:** `src/Sales/Sales.Infrastructure/Orders/OrderReadService.cs`, `src/Sales/Sales.Infrastructure/DependencyInjection/SalesInfrastructureExtensions.cs`, `tests/IntegrationTests/Sales/Persistence/SalesOrderReadModelTests.cs`
- **Test level:** Infrastructure integration

## Evidence

- **Focused test:** `SalesOrderReadModelTests`
- **Red:** authored the list projection test before the service; the first command compiled but was skipped because Docker was unavailable, so an executable Red was not observed. This provider-prerequisite deviation is recorded rather than treated as Red evidence.
- **Green:** `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~SalesOrderReadModelTests` with `KEDR_SALES_TEST_POSTGRES_CONNECTION` targeting an isolated PostgreSQL 18 schema; 4 passed, 0 failed, 0 skipped, including a 150-line aggregate projection.
- **Refactor:** feature-specific seed IDs removed cross-class fixture collisions; the focused PostgreSQL run remained 4/4 passing.
- **Regression:** `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~Sales.Persistence` against an isolated PostgreSQL 18 schema; 7 passed, 0 failed, 0 skipped.

## Work

- [x] **Red:** add the smallest PostgreSQL integration test for one SC-001 list projection and run it; the provider prerequisite caused the documented Red deviation above.
- [x] **Red:** add focused cases for detail/not-found, exact counterparty filtering, empty results, soft-deleted-counterparty behavior, and excluded diagnostics as required by SC-002–SC-004.
- [x] **Green:** implement only enough no-tracking list/detail projection behavior to pass the current failing case; join only required `Counterparty`, `OrderLine`, and `OneCOrderSync` data.
- [x] **Green:** apply agreed ordering, filters, and pagination before materialization; return an empty collection or missing detail as agreed.
- [x] **Green:** register the abstraction in Sales Infrastructure and rerun the focused test after each increment.
- [x] **Refactor:** remove redundant loading, keep internal diagnostics out of projections, and inspect query plan/index usage if EN-001 defines a performance target.
- [x] **Regression:** run all `Sales.Persistence` integration tests and verify no tracked modifications or 1C calls.
- [x] Record actual Red, Green, refactor, and regression results in `Evidence`.

## Checkpoint

The projection scopes results correctly and loads only agreed response data.
