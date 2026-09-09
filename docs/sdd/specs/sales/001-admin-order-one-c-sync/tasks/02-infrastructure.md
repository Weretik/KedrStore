# Phase 02 — Infrastructure

- [x] [T040 — EF Core persistence mapping](infrastructure/02.1-order-persistence.md)
- [x] [T050 — Sales database migration](infrastructure/02.2-sales-migration.md)
- [x] [T060 — 1C SOAP write adapter](infrastructure/02.3-one-c-write-adapter.md)
- [x] [T080 — Infrastructure integration tests](infrastructure/02.5-infrastructure-tests.md)

## Checkpoint

The Sales schema persists one durable delivery record for each order, and SOAP concerns remain at the Infrastructure boundary. The scheduled delivery worker is intentionally implemented in Phase 05 after the HTTP creation path is complete.
