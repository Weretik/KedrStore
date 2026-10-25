# 005 — Saved Admin order drafts

- **Module:** Sales
- **Consumer:** React Admin
- **Status:** Implemented in commit `b3797c1`; PostgreSQL persistence tests added 2026-10-25 (not yet run against PostgreSQL); review is still required
- **Baseline checked:** 2026-10-21

A draft is a record separate from an Order, with a title, optional note, customer, and lines. It has no order number and is not sent to 1C. It can be edited, deleted, or reused as the basis for checkout.

- [Rules and scenarios](requirements/behavior.md)
- [Data model and design](design/backend.md)
- [API decisions](contracts/api-contract.md)
- [Tasks](tasks/README.md)
- [Traceability and readiness](traceability.md)
- [Shared 004/005 responsibility audit](../004-admin-order-workspace/code-audit/audit.md)
- [Structural remediation](../006-admin-workspace-structure-remediation/README.md)
- [2026-10-22 change: creation date range](date-filter-2026-10-22.md) — planned; code and OpenAPI have not changed

Related separate checkout increment: [004](../004-admin-order-workspace/README.md).

- [SDD readiness](checklist/spec-readiness.md)
