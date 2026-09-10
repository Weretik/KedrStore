# EN-002 — Define customer query contracts

- **Task ID:** EN-002
- **Enables:** SC-001, SC-002, SC-003
- **Depends on:** EN-001
- **Exact paths:** `src/Sales/Sales.Application/Contracts/Customers/ICustomerReadService.cs`, `src/Sales/Sales.Application/Features/Customers/GetList/DTOs/`, `src/Sales/Sales.Application/Features/Customers/GetById/DTOs/`
- **Test level:** Application compile/contract verification

## Why this is an enabler

The list and detail handlers share a privacy-bounded read abstraction. Defining it first keeps EF and Identity persistence out of Application.

## Work

- [x] Define one Application read abstraction for active-customer list/detail projections.
- [x] Define list-row, detail, price-rule, paging, and filter records only when required by EN-001.
- [x] Exclude Domain entities, EF types, deletion/audit fields, `IdentityUserId`, and personal data unless explicitly agreed.
- [x] Keep order reading in feature 002 rather than coupling order graphs to customer projections.
- [x] Pass `CancellationToken` through every read operation.

## Test-first exception and replacement verification

- **Reason:** compile-time contracts precede handler and infrastructure behavioral tests.
- **Replacement check:** build `src/Sales/Sales.Application/Sales.Application.csproj` and compile focused test fixtures.
- **Result:** `dotnet build src/Sales/Sales.Application/Sales.Application.csproj --no-restore` passed; focused fixtures compiled and later passed.

## Checkpoint

List and detail use cases have stable, minimal Application contracts matching the privacy decision.
