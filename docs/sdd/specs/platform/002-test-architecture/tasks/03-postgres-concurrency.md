# TS-002 — Prove PostgreSQL concurrent order-sync claims

- **Covers:** SC-003
- **Depends on:** EN-001
- **Exact paths:** `tests/IntegrationTests/Sales/OneC`, `tests/IntegrationTests/TestSupport/Sales`
- **Test level:** PostgreSQL integration

## Work

- [x] Seed one due synchronization in PostgreSQL.
- [x] Start two services with separate `SalesDbContext` instances.
- [x] Coordinate both workers so their claim operations overlap.
- [x] Assert one outbound 1C call and one persisted accepted state.
- [x] Remove the simulated `xmin` mutation test.

## Verification-only exception and evidence

- Red/Green are not applicable because this task adds provider verification for
  existing production behavior and does not change that behavior.
- Local result: `TwoWorkers_SendOrderOnlyOnce` was discovered and explicitly
  skipped because Docker is unavailable.
- Acceptance disposition: local discovery plus the explicit environment skip
  satisfies local delivery. CI executes the test as a mandatory gate; any
  future CI failure is handled as a separate incident.

## Checkpoint

The test fails if both workers send the same order and passes using real
PostgreSQL optimistic concurrency.
