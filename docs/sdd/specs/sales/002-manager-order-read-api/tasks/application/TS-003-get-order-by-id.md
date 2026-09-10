# TS-003 — Get manager order by ID

- **Task ID:** TS-003
- **Covers:** SC-002, SC-004
- **Depends on:** TS-001
- **Exact paths:** `src/Sales/Sales.Application/Features/Orders/GetById/`, `tests/UnitTests/Sales/Application/GetOrderByIdQueryHandlerTests.cs`
- **Test level:** Application

## Evidence

- **Focused test:** `GetOrderByIdQueryHandlerTests`
- **Red:** combined focused handler run compiled and failed 2 detail/not-found delegation assertions against the absent orchestration while its 2 validator cases passed.
- **Green:** combined `GetOrderListQueryHandlerTests|GetOrderByIdQueryHandlerTests` run passed 9/9.
- **Refactor:** reran the same focused suite after simplifying delegation; 9/9 passed.
- **Regression:** `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~Sales.Application`; 21 passed, 0 failed.

## Work

- [x] **Red:** add the smallest handler test for SC-002 and run it to confirm failure because detail orchestration is absent.
- [x] **Red:** add focused cases for invalid ID, not found, read failure, and cancellation required by SC-002/SC-004.
- [x] **Green:** add the detail query, agreed result DTO, and identifier validation.
- [x] **Green:** return the agreed projection or explicit not-found through `IOrderReadService`, rerunning the focused test.
- [x] **Refactor:** simplify mapping while keeping internal diagnostics, EF types, and Domain entities outside the result.
- [x] **Regression:** run the Sales Application test subset.
- [x] Record actual Red, Green, refactor, and regression results in `Evidence`.

## Checkpoint

The use case returns the exact detail projection and distinguishes a missing order from success.
