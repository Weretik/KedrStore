# TS-004 — Add manager-order read endpoints

- **Task ID:** TS-004
- **Covers:** SC-001, SC-002, SC-003, SC-004, SC-005
- **Depends on:** TS-002, TS-003, EN-003
- **Exact paths:** `src/Sales/Sales.Api/Controllers/AdminOrdersController.cs`, `src/Sales/Sales.Api/Contracts/Orders/`, `tests/IntegrationTests/Sales/Api/AdminOrderApiTests.cs`
- **Test level:** API integration

## Evidence

- **Focused test:** manager-order read cases in `AdminOrderApiTests`
- **Red:** `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~ManagerOrderReadApiTests`; 8 failed on missing GET routes/access and 3 pre-endpoint cases passed.
- **Green:** same command; 11 passed, 0 failed.
- **Refactor:** explicit query parameter names and transport metadata aligned runtime OpenAPI and preserved blank-input validation; focused suite remained 11/11 passing.
- **Regression:** `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~Sales.Api`; 26 passed, 0 failed.

## Work

- [x] **Red:** add the smallest HTTP integration test for the first agreed route and run it to confirm the expected missing-route/behavior failure.
- [x] **Red:** add focused failing tests for response mapping, validation/not-found, authentication, authorization, and scope required by SC-001–SC-005.
- [x] **Green:** add thin endpoint methods; map inputs to queries, pass cancellation, and use the established Ardalis.Result mapping.
- [x] **Green:** add only required HTTP DTOs/mappers, apply the agreed policy, and declare response types/statuses matching EN-003.
- [x] **Refactor:** preserve existing create, sync-status, and retry behavior while applying `CanManageOrders` only to the new read routes.
- [x] **Regression:** run `AdminOrderApiTests` and the Sales API subset.
- [x] Record actual Red, Green, refactor, and regression results in `Evidence`.

## Checkpoint

Every route is thin, authorized, cancellation-aware, and contract-compliant.
