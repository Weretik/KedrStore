# TS-002 — Get manager-order lists

- **Task ID:** TS-002
- **Covers:** SC-001, SC-003
- **Depends on:** TS-001
- **Exact paths:** `src/Sales/Sales.Application/Features/Orders/GetList/`, `tests/UnitTests/Sales/Application/GetOrderListQueryHandlerTests.cs`
- **Test level:** Application

## Evidence

- **Focused test:** `GetOrderListQueryHandlerTests`
- **Red:** combined focused handler run compiled and failed 2 list orchestration assertions against the absent delegation while its 3 validator cases passed.
- **Green:** combined `GetOrderListQueryHandlerTests|GetOrderByIdQueryHandlerTests` run passed 9/9.
- **Refactor:** reran the same focused suite after simplifying delegation; 9/9 passed.
- **Regression:** `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~Sales.Application`; 21 passed, 0 failed.

## Work

- [x] **Red:** add the smallest handler test for SC-001 and run it to confirm failure because list orchestration is absent.
- [x] **Red:** add focused cases for exact counterparty scope, invalid input, read failure, and cancellation; persistence tests cover empty/exclusion behavior at the layer that applies the filter.
- [x] **Green:** add the query, optional counterparty scope, agreed filters/paging, validator, and result DTO.
- [x] **Green:** delegate selection to `IOrderReadService`, return explicit Ardalis.Result outcomes, and rerun the focused test.
- [x] **Refactor:** simplify mapping and validation without introducing EF or Domain entity access.
- [x] **Regression:** run the Sales Application test subset.
- [x] Record actual Red, Green, refactor, and regression results in `Evidence`.

## Checkpoint

The query supports the agreed collections and cannot leak orders outside the requested counterparty scope.
