# 003 — Customer read API

- **Module:** Sales
- **Type:** feature
- **Status:** delivered
- **Owner:** Sales team
- **Created:** 2026-09-10

API consumers need to browse existing customers and inspect one customer’s current Sales profile. This feature exposes anonymous read-only projections of the existing `Counterparty` model and its category price-type rules; it does not alter customer synchronization, Identity users, or price calculation.

## Requirements

- [Overview and boundaries](requirements/overview.md)
- [Customer reading](requirements/customer-reading.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)
- [API contract](contracts/api-contract.md)
- [Versioned OpenAPI](../../../contracts/sales/customer-read.openapi.yaml)

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Task graph](tasks/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## Related documentation

- [Manager order read API](../002-manager-order-read-api/README.md)
- [Sales customer synchronization runbook](../../../operations/jobs/sales-one-c-runbook.md)
- [Current aggregate API contract](../../../contracts/openapi.yaml)
- [Sales module ownership](../../../architecture/overview/modules.md)

## Delivery report

SC-001 through SC-004 and EN-001 through EN-003 / TS-001 through TS-006 are complete. `GET /api/admin/customers` returns a compact active-customer page ordered by name and counterparty ID; `GET /api/admin/customers/{counterpartyId}` returns the approved current profile and category price rules. Both routes currently allow anonymous access.

The delivery gate passed restore, a warning-free build, 70 Unit tests, 10 Architecture tests, and 83 Integration tests with no skips after the anonymous-access amendment. PostgreSQL checks used isolated schemas that were removed after verification. The feature OpenAPI validates; Redocly reports only its advisory `info-license` warning. No migration was required.

## Change notes

- 2026-09-10 — Initial draft for customer list and detail reads. No code or machine-readable contract is added.
- 2026-09-10 — Agreed compact list/full detail DTOs, paging/order, soft-delete behavior, and separate paged order loading.
- 2026-09-10 — Delivered list/detail queries, PostgreSQL projections, HTTP endpoints, versioned OpenAPI, and focused/full regression evidence.
- 2026-09-10 — Changed both customer-read routes to anonymous access for the current rollout stage and updated contract/scenario evidence.
