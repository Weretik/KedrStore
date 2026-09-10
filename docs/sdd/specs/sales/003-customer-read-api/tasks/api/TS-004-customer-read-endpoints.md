# TS-004 — Add customer-read endpoints

- **Task ID:** TS-004
- **Covers:** SC-001, SC-002, SC-003, SC-004
- **Depends on:** TS-002, TS-003, EN-003
- **Exact paths:** `src/Sales/Sales.Api/Controllers/AdminCustomersController.cs`, `src/Sales/Sales.Api/Contracts/Customers/`, `tests/IntegrationTests/Sales/Api/AdminCustomerApiTests.cs`
- **Test level:** API integration

## Evidence

- **Focused test:** `AdminCustomerApiTests`
- **Red:** focused run compiled 12 tests and failed 9 with route-level `404` because customer endpoints were absent; the 3 independent cases passed.
- **Green:** after the access amendment, the anonymous-route Red failed 2/2 with `401`; applying `[AllowAnonymous]` made the full focused suite pass 11/11.
- **Refactor:** public request/response types remain in separate files and the controller only maps transport contracts to queries; focused tests remained Green.
- **Regression:** combined `Sales.Api`, `Sales.Persistence`, and `Platform.Api` run passed 55/55.

## Work

- [x] **Red:** add the smallest HTTP integration test for the first agreed customer route and run it to confirm the expected missing-route/behavior failure.
- [x] **Red:** add focused failing tests for response mapping, validation/not-found, soft-delete privacy, and anonymous access required by SC-001–SC-004.
- [x] **Green:** add thin endpoint methods one failing case at a time; map inputs to queries, pass cancellation, and use established Ardalis.Result mapping.
- [x] **Green:** add only required HTTP DTOs/mappers, apply the agreed anonymous access, and declare response types/statuses matching EN-003.
- [x] **Refactor:** remove controller/mapping duplication without broadening the approved personal-data boundary.
- [x] **Regression:** run `AdminCustomerApiTests` and the Sales API subset.
- [x] Record actual Red, Green, refactor, and regression results in `Evidence`.

## Checkpoint

Every route is thin, explicitly anonymous, cancellation-aware, and privacy/contract-compliant.
