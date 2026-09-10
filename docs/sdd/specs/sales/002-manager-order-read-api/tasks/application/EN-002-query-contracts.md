# EN-002 — Define manager-order query contracts

- **Task ID:** EN-002
- **Enables:** SC-001, SC-002, SC-003, SC-004
- **Depends on:** EN-001
- **Exact paths:** `src/Sales/Sales.Application/Contracts/Orders/IOrderReadService.cs`, `src/Sales/Sales.Application/Features/Orders/GetList/DTOs/`, `src/Sales/Sales.Application/Features/Orders/GetById/DTOs/`
- **Test level:** Application compile/contract verification

## Why this is an enabler

List and detail handlers share a read abstraction and projections. Defining that boundary first keeps EF types out of Application and supports focused test fakes.

## Work

- [x] Inspect `IOrderSyncStatusReader`; reuse concepts without expanding its sync-only responsibility.
- [x] Define one Application read abstraction for the agreed list/detail projections.
- [x] Define list-row, detail, line, counterparty, sync, paging, and filter records only when required by EN-001.
- [x] Exclude Domain entities, EF types, SOAP diagnostics, retry audits, and notification fields.
- [x] Pass `CancellationToken` through every read operation.

## Test-first exception and replacement verification

- **Reason:** compile-time contracts precede handler and infrastructure behavioral tests.
- **Replacement check:** build `src/Sales/Sales.Application/Sales.Application.csproj` and compile focused test fixtures.
- **Result:** completed 2026-09-10; `dotnet build src/Sales/Sales.Application/Sales.Application.csproj` passed with 0 warnings and 0 errors.

## Checkpoint

List and detail use cases have stable, minimal Application contracts matching the agreed response boundary.
