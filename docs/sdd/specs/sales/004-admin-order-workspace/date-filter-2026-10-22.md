# Change 2026-10-22 — order date range

**Status:** SDD, code, and versioned OpenAPI have not yet changed. Addendum to [004 Admin workspace](README.md); historical R-001–R-007, SC-001–SC-006, and evidence remain unchanged. Shared increment ID with Admin frontend: `order-workspace-2026-10-22`.

## Requirements and acceptance

- **R-008.** `getAdminWorkspaceOrders` accepts optional `createdFrom` and `createdTo` in calendar-date format `YYYY-MM-DD`. Boundaries are inclusive in the `Europe/Kyiv` timezone; convert them in SQL to a half-open UTC interval `[start of createdFrom, start of the day after createdTo)`. This correctly covers daylight-saving transition days. Either boundary may be supplied alone; `createdFrom > createdTo` returns 400.
- **R-009.** The filter is combined with existing `searchTerm` and `counterpartyId` using AND; `totalRecords` is calculated after all conditions and before `Skip/Take`, preserving stable sorting. The same endpoint serves the main page and the table in customer details; in the latter, `counterpartyId` is required frontend context.
- **R-010.** Parameters do not change the record DTO, authorization decision, create idempotency, or database data. No migration is needed. Propose an index only after `EXPLAIN` on relevant volume.

| ID | Given / When / Then |
| --- | --- |
| **SC-007** | Given orders on and outside the range in Kyiv time; when Admin requests `createdFrom`/`createdTo`; then only records in the included calendar days are returned, with the correct `totalRecords`. |
| **SC-008** | Given a customer has orders on different dates and other customers also have records; when the request includes `counterpartyId` and a range; then the page contains only that customer's records in the range. |
| **SC-009** | Given the boundaries are reversed or have an invalid format; when Admin requests the list; then 400 is returned without a SQL query using invalid conditions. |

## Contract, design, and sequence

Canonical operation: `GET /api/admin/order-workspace/orders`, `operationId: getAdminWorkspaceOrders` in `docs/sdd/contracts/sales/admin-order-workspace.openapi.yaml` and aggregated `docs/sdd/contracts/openapi.yaml`. Plan query parameters `createdFrom?: string(date)`, `createdTo?: string(date)`; the response remains the existing `OrderPage`, and 400 uses the existing safe format. Before implementation, update only provider OpenAPI and contract tests; after recording the revision, hand it to frontend for the standard snapshot sync. The controller's current `[AllowAnonymous]` follows the prior-stage decision and is unaffected.

The request flows through `AdminOrderWorkspaceController` → `GetWorkspaceOrdersQuery` → `WorkspaceOrderReader`. Validate boundary format/order before the reader; calculate UTC boundaries in one place. Do not apply a function to `CreatedAtUtc` in SQL `WHERE`, preserving index usability. With neither boundary supplied, retain current behavior.

| Path | State | Responsibility |
| --- | --- | --- |
| `docs/sdd/contracts/sales/admin-order-workspace.openapi.yaml` | existing | versioned HTTP date contract |
| `src/Sales/Sales.Api/Controllers/AdminOrderWorkspaceController.cs` | existing | query binding and 400 |
| `src/Sales/Sales.Application/Features/Orders/AdminWorkspace/Orders/GetList/` | existing | validation/condition passing |
| `src/Sales/Sales.Infrastructure/Orders/Admin/Read/WorkspaceOrderReader.cs` | existing | UTC conditions, count, and page |
| `tests/IntegrationTests/Sales/Api/` | existing directory | focused API/SQL boundary and pagination tests |
| `docs/sdd/features/sales/admin-order-workspace.md` | existing | reconcile feature map after implementation |

| Task | Depends on | Work | Evidence/checkpoint |
| --- | --- | --- | --- |
| **EN-002** | — | Align OpenAPI parameters and `Europe/Kyiv` timezone; record provider revision | contract lint, exact DST/400 examples |
| **TS-005** | EN-002 | Add binding, validation, and reader filter in the listed paths | Red → Green: API/SQL tests for boundaries, AND conditions, count |
| **TS-006** | TS-005 | Reconcile feature map, contract, and targeted regression | traceability/evidence, no full suite without request |

| Rules | Scenarios | Tasks | Level |
| --- | --- | --- | --- |
| R-008–R-010 | SC-007–SC-009 | EN-002, TS-005–TS-006 | contract/API/SQL integration |

**Readiness:** Product decision on date range and timezone is accepted. There is no blocker to the provider contract; this document does not authorize implementation.
