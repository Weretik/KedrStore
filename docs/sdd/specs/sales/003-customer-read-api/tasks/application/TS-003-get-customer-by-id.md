# TS-003 — Get customer by counterparty ID

- **Task ID:** TS-003
- **Covers:** SC-002, SC-003
- **Depends on:** TS-001
- **Exact paths:** `src/Sales/Sales.Application/Features/Customers/GetById/`, `tests/UnitTests/Sales/Application/GetCustomerByIdQueryHandlerTests.cs`
- **Test level:** Application

## Evidence

- **Focused test:** `GetCustomerByIdQueryHandlerTests`
- **Red:** combined customer-handler run compiled 11 tests and failed the 2 detail handler cases because the temporary handler returned the expected not-implemented error; identifier and privacy tests passed.
- **Green:** combined focused customer-handler run passed 11/11.
- **Refactor:** handler delegates directly to `ICustomerReadService`, preserves success/not-found/error, and forwards the cancellation token; focused tests remained Green.
- **Regression:** `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~Sales.Application` passed 32/32.

## Work

- [x] **Red:** add the smallest handler test for SC-002 and run it to confirm failure because detail orchestration is absent.
- [x] **Red:** add focused failing cases for invalid ID, unknown ID, deleted ID, read failure, privacy exclusions, and cancellation required by SC-002/SC-003.
- [x] **Green:** add the detail query/result DTO and identifier validation one failing case at a time.
- [x] **Green:** return explicit not-found or the approved projection through `ICustomerReadService`, including personal data/price rules only when EN-001 permits them.
- [x] **Refactor:** simplify mapping while keeping excluded PII, EF types, and Domain entities outside the result.
- [x] **Regression:** run the Sales Application test subset.
- [x] Record actual Red, Green, refactor, and regression results in `Evidence`.

## Checkpoint

The use case distinguishes missing/deleted customers and returns only the approved profile.
