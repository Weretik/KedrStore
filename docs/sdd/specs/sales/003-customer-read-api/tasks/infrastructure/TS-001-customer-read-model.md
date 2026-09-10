# TS-001 — Implement customer read model

- **Task ID:** TS-001
- **Covers:** SC-001, SC-002, SC-003
- **Depends on:** EN-002
- **Exact paths:** `src/Sales/Sales.Infrastructure/Customers/CustomerReadService.cs`, `src/Sales/Sales.Infrastructure/DependencyInjection/SalesInfrastructureExtensions.cs`, `tests/IntegrationTests/Sales/Persistence/SalesCustomerReadModelTests.cs`
- **Test level:** Infrastructure integration

## Evidence

- **Focused test:** `SalesCustomerReadModelTests`
- **Red:** focused PostgreSQL run failed 1/1 with `ICustomerReadService` not registered, confirming missing read behavior.
- **Green:** same focused run passed 4/4 against an isolated PostgreSQL schema, no skips.
- **Refactor:** retained one no-tracking projection service, global soft-delete filtering, deterministic ordering, and bounded DTO projections; 4/4 remained Green.
- **Regression:** combined `Sales.Persistence`, `Sales.Api`, and `Platform.Api` run passed 55/55 against an isolated PostgreSQL schema.

## Work

- [x] **Red:** add the smallest PostgreSQL integration test for the SC-001 active-customer list and run it to confirm the expected missing-behavior failure.
- [x] **Red:** add focused failing cases for detail, unknown/deleted identifiers, empty results, privacy exclusions, and optional price rules required by SC-002/SC-003.
- [x] **Green:** implement only enough no-tracking list/detail projection behavior to pass the current failing case.
- [x] **Green:** apply the active-counterparty predicate, agreed ordering/search/pagination, optional price-rule loading, and register the abstraction one increment at a time.
- [x] **Refactor:** remove redundant loading, keep excluded fields outside projections, and inspect indexes only if an agreed performance target requires it.
- [x] **Regression:** run all `Sales.Persistence` integration tests and verify no Identity/1C calls or tracked modifications.
- [x] Record actual Red, Green, refactor, and regression results in `Evidence`.

## Checkpoint

The projection returns only active customers and only agreed fields.
