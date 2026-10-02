# 003 — Missing product photos

- **Module:** Catalog
- **Type:** feature
- **Status:** verified
- **Owner:** Catalog team
- **Created:** 2026-09-23

Provide a fast anonymous administration API for active products whose stored
photo URL is currently unusable. A one-shot Host.Jobs command probes the CDN
and persists the latest result; the HTTP request reads that state from
PostgreSQL and never waits for the CDN. Deployment creates or updates the
corresponding Cloud Run Job, while its schedule remains operator-owned.

## Requirements

- [Overview and scope](requirements/overview.md)
- [Photo availability tracking](requirements/photo-availability.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)
- [API and integration contract](contracts/api-contract.md)

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Task graph](tasks/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

Behavior tasks used `Red -> Green -> Refactor -> Regression -> evidence`.
Implementation and delivery evidence are recorded in the task files and
traceability matrix.

## Change notes

- 2026-09-23 — Initial specification: persisted CDN checks, anonymous missing-photo API, Host.Jobs command, and Cloud Run Job deployment wiring.
- 2026-09-23 — Implemented and verified the domain state, PostgreSQL migration, bounded CDN probe, import reconciliation, one-shot job, database-only anonymous API, contracts, operations guide, and Cloud Run Job definition.
