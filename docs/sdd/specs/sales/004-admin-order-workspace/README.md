# 004 — Admin order workspace

- **Module:** Sales
- **Consumer:** React Admin
- **Status:** implemented in commit `b3797c1`; PostgreSQL verification and review are still required
- **Baseline checked:** 2026-10-21

Separate Admin operations let a manager search and view orders, calculate prices on the server, and place an order from the current product selection. The existing `/api/admin/orders` routes and DTOs do not change in this increment.

- [Rules and scenarios](requirements/behavior.md)
- [Backend design](design/backend.md)
- [API decisions](contracts/api-contract.md)
- [Tasks](tasks/README.md)
- [Traceability and readiness](traceability.md)
- [Code responsibility audit](code-audit/audit.md)
- [Structural remediation](../006-admin-workspace-structure-remediation/README.md)
- [2026-10-22 change: order date range](date-filter-2026-10-22.md) — planned; code and OpenAPI have not yet changed

The draft model is defined in [005](../005-admin-order-drafts/README.md). The actual state of current orders is described in the [Sales feature map](../../../features/sales/admin-orders.md).

- [SDD readiness](checklist/spec-readiness.md)
