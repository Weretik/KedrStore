# TS-002 — Get active-customer list

- **Task ID:** TS-002
- **Covers:** SC-001
- **Depends on:** TS-001
- **Exact paths:** `src/Sales/Sales.Application/Features/Customers/GetList/`, `tests/UnitTests/Sales/Application/GetCustomerListQueryHandlerTests.cs`
- **Test level:** Application

## Evidence

- **Focused test:** `GetCustomerListQueryHandlerTests`
- **Red:** combined customer-handler run compiled 11 tests and failed the 2 list handler cases because the temporary handler returned the expected not-implemented error; validators passed.
- **Green:** combined focused customer-handler run passed 11/11.
- **Refactor:** handler delegates directly to `ICustomerReadService`, preserves its `Result`, and forwards the cancellation token; focused tests remained Green.
- **Regression:** `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~Sales.Application` passed 32/32.

## Work

- [x] **Red:** add the smallest handler test for SC-001 and run it to confirm failure because list orchestration is absent.
- [x] **Red:** add focused failing cases for empty list, invalid page/search/sort/filter input, read failure, and cancellation.
- [x] **Green:** add the query, agreed filters/paging, validator when needed, and result DTO one failing case at a time.
- [x] **Green:** delegate selection to `ICustomerReadService`, return explicit Ardalis.Result outcomes, and rerun the focused test after each increment.
- [x] **Refactor:** simplify mapping and validation without accessing EF or Domain entities.
- [x] **Regression:** run the Sales Application test subset.
- [x] Record actual Red, Green, refactor, and regression results in `Evidence`.

## Checkpoint

The query exposes only the agreed active-customer list boundary.
