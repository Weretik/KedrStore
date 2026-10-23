# Change 2026-10-22 — draft creation date range

**Status:** SDD, code, and OpenAPI are unchanged. Addendum to [005 Admin drafts](README.md); previous R-001–R-006, SC-001–SC-007, and completed tasks remain historical. Shared increment with frontend: `order-workspace-2026-10-22`.

## Requirements and scenarios

- **R-007.** `getAdminOrderDrafts` accepts optional `createdFrom` and `createdTo` as `YYYY-MM-DD`; drafts are filtered by immutable `CreatedAtUtc`, not `UpdatedAtUtc`. Both boundaries are inclusive in `Europe/Kyiv`; either may be supplied alone. A reversed/invalid range returns 400. SQL uses half-open UTC boundaries.
- **R-008.** The range is combined with search and `counterpartyId`; `totalRecords` is counted before pagination. Existing `UpdatedAtUtc` sorting remains the default unless the user changes sort. DTO, data model, and draft deletion mechanism are unchanged; no migration is needed.

| ID | Given / When / Then |
| --- | --- |
| **SC-008** | Given a draft was created in the selected range and updated later outside it; when Admin sets dates; then the draft is included based on creation date and the count matches the conditions. |
| **SC-009** | Given drafts for different customers fall on range boundaries; when Admin combines dates, search, and `counterpartyId`; then page, count, and stable ordering reflect every condition. |
| **SC-010** | Given the format or order of boundaries is invalid; when Admin requests the list; then 400 is returned without changing data. |

## Contract, design, and tasks

Canonical operation: `GET /api/admin/order-workspace/drafts`, `operationId: getAdminOrderDrafts` in `docs/sdd/contracts/sales/admin-order-drafts.openapi.yaml`. Add `createdFrom?: string(date)`/`createdTo?: string(date)` to provider OpenAPI; response remains `DraftPage`. The current `[AllowAnonymous]` on the workspace controller remains. After the provider revision, frontend syncs the snapshot through the standard workflow.

`AdminOrderWorkspaceController` passes the range to `GetWorkspaceDraftsQuery`; `WorkspaceDraftReader` applies UTC conditions to `CreatedAtUtc` before `LongCountAsync` and `Skip/Take`. Boundary and validation rules must be aligned with the [order filter](../004-admin-order-workspace/date-filter-2026-10-22.md), without duplicating timezone rules.

| Path | State | Responsibility |
| --- | --- | --- |
| `docs/sdd/contracts/sales/admin-order-drafts.openapi.yaml` | existing | provider parameters |
| `src/Sales/Sales.Api/Controllers/AdminOrderWorkspaceController.cs` | existing | binding |
| `src/Sales/Sales.Application/Features/Orders/AdminWorkspace/Drafts/GetList/` | existing | validation/passing conditions |
| `src/Sales/Sales.Infrastructure/Orders/Admin/Read/WorkspaceDraftReader.cs` | existing | filter, count, pagination |
| `tests/IntegrationTests/Sales/Api/` | existing directory | focused API/SQL tests |
| `docs/sdd/features/sales/admin-order-workspace.md` | existing | reconcile after implementation |

| Task | Depends on | Work | Evidence/checkpoint |
| --- | --- | --- | --- |
| **EN-002** | 004/EN-002 | Align draft OpenAPI with the same date boundaries | contract lint, example updated outside range |
| **TS-005** | EN-002 | Implement query/reader filter | Red → Green: API/SQL tests for creation vs update, AND, count, 400 |
| **TS-006** | TS-005 | Reconcile feature map and targeted regression | traceability/evidence |

| Rules | Scenarios | Tasks | Level |
| --- | --- | --- | --- |
| R-007–R-008 | SC-008–SC-010 | EN-002, TS-005–TS-006 | contract/API/SQL integration |

**Readiness:** `CreatedAtUtc`, range behavior, and `Europe/Kyiv` are accepted; implementation has not started.
