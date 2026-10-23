# 006 — Sales Admin workspace responsibility separation

- **Date:** 2026-10-21.
- **Scope:** code structure in SDDs [004](../004-admin-order-workspace/README.md) and [005](../005-admin-order-drafts/README.md) only; HTTP, OpenAPI, database model, and 1C job are unchanged.
- **Basis:** [004/005 audit](../004-admin-order-workspace/code-audit/audit.md) and [layer rules](../../../architecture/overview/layers.md).

## Requirements, tasks, and verification

| Requirement | Finding | Task and target | Verification |
| --- | --- | --- | --- |
| AR-001: one Application handler per use case; API sends a Mediator request | AF-001 | RM-001: nine query/command-handler pairs under `Sales.Application/Features/Orders/AdminWorkspace/{Orders,Drafts}/<scenario>/`; controller uses `ISender`. | Build Sales.Api and run layer architecture tests. |
| AR-002: Infrastructure has narrow adapters by role | AF-001 | RM-002: replace partial service with `Admin/Read/{WorkspaceOrderReader,WorkspaceDraftReader}`, `Admin/Checkout/WorkspaceOrderCreator`, `Admin/Drafts/{WorkspaceDraftWriter,WorkspaceDraftDetails}`, `Admin/Pricing/AdminWorkspacePricing`, `Admin/Idempotency/WorkspaceIdempotency`. | DI compilation, targeted use-case tests, and PostgreSQL verification before rollout. |
| AR-003: contracts do not mix independent flows | AF-002 | RM-003: group DTOs in `Workspace{Quote,Order,Draft,Page}Contracts.cs` and `WorkspaceInputLine.cs`; separate read, pricing, and write ports as `IWorkspace*.cs`. | Compile and check OpenAPI operations are unchanged. |
| AR-004: remove dead code | AF-003 | RM-004: remove the unused local `lines` projection from details reads; retain the SQL line query. | Build and run the relevant read test. |

**Order:** RM-003 → RM-002 → RM-001; do RM-004 with the read refactor. Roll back as one group with DI and controller changes; no migrations or data changes. The [implemented feature map](../../../features/sales/admin-order-workspace.md) shows actual paths. Current statuses and evidence remain in the 004/005 audit to avoid duplicating history.
