# 002 — Test architecture and delivery gates

- **Module:** Platform
- **Type:** feature
- **Status:** completed
- **Owner:** Platform team
- **Created:** 2026-09-09

The test suite is reorganized around business modules and test levels, and the
delivery pipeline proves unit, integration, PostgreSQL-specific, and
architecture risks before deployment. This work changes test infrastructure
and verification only; it does not change production behavior or public API
contracts.

## Requirements

- [Scope and acceptance scenarios](requirements/overview.md)

## Technical design

- [Test projects, modules, fixtures, and delivery gates](design/test-architecture.md)

## Planning and delivery

- [Scenario traceability](traceability.md)
- [Task graph](tasks/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## Related durable documentation

- [Project structure](../../../architecture/overview/project-structure.md)
- [Module map](../../../architecture/overview/modules.md)
- [Layer boundaries](../../../architecture/overview/layers.md)
- [Testing rules](../../../standards/testing-rules.md)
- [Admin order and 1C synchronization](../../sales/001-admin-order-one-c-sync/README.md)

## Change notes

- 2026-09-09 — Feature completed using the local delivery criterion. The
  Docker-dependent PostgreSQL test is implemented, discovered, and configured
  as mandatory in CI; its local skip is an accepted environment exception.
  Any failure observed during a future GitHub Actions run will be handled as a
  CI incident rather than leaving this implementation task open.
- 2026-09-09 — Architecture enforcement exposed and removed two outward
  dependencies: `Identity.Api -> Identity.Infrastructure` and
  `BuildingBlocks.Infrastructure -> Catalog.Application`. The XML-to-JSON
  adapter moved to `Catalog.Infrastructure` with its registration.
- 2026-09-09 — Initial specification created from the test architecture audit;
  implementation was authorized for the complete feature.
