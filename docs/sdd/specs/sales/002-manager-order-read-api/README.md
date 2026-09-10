# 002 — Manager order read API

- **Module:** Sales
- **Type:** feature
- **Status:** delivered
- **Owner:** Sales team
- **Created:** 2026-09-10

Sales managers need to browse persisted manager orders, open one order with its lines and delivery state, and browse orders for one counterparty. This feature exposes read-only projections of the existing Sales order model; it does not change order creation, 1C delivery, or customer synchronization.

## Requirements

- [Overview and boundaries](requirements/overview.md)
- [Order reading](requirements/order-reading.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)
- [API contract](contracts/api-contract.md)
- [Versioned OpenAPI](../../../contracts/sales/manager-order-read.openapi.yaml)

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Task graph](tasks/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## Related documentation

- [Admin order and 1C synchronization](../001-admin-order-one-c-sync/README.md)
- [Current aggregate API contract](../../../contracts/openapi.yaml)
- [Sales module ownership](../../../architecture/overview/modules.md)

## Change notes

- 2026-09-10 — Initial draft for manager-order list, detail, and counterparty-scoped list reads. No code or machine-readable contract is added.
- 2026-09-10 — Agreed compact list/full detail DTOs, counterparty filtering, paging/order, historical soft-deleted-customer behavior, and `CanManageOrders` access.
- 2026-09-10 — Delivered list/detail queries, PostgreSQL projections, `CanManageOrders` HTTP endpoints, versioned OpenAPI, and focused/full regression evidence.

## Delivery report

SC-001 through SC-005 and EN-001 through EN-003 / TS-001 through TS-006 are complete. `GET /api/admin/orders` returns a newest-first compact page with optional exact counterparty filtering; `GET /api/admin/orders/{orderId}` returns the approved detail projection. Both routes require `PolicyNames.CanManageOrders` and preserve existing create, sync-status, and retry routes.

The delivery gate passed restore, a warning-free build, 59 Unit tests, 10 Architecture tests, and 67 Integration tests. PostgreSQL checks ran in isolated schemas that were removed after verification. TS-001's first Red attempt was skipped when Docker was unavailable; later focused and regression tests executed against PostgreSQL 18 with no failures or skips. Repository-wide aggregate Redocly lint still reports pre-existing errors in the older admin-order sync contract; this feature's contract validates, and focused aggregate plus runtime checks pass.
