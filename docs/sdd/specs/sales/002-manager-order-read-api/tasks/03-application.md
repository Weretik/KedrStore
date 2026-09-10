# Phase 03 — Application

## Ordered tasks

1. After EN-001, execute [EN-002 — Define manager-order query contracts](application/EN-002-query-contracts.md).
2. Wait for Infrastructure TS-001.
3. Execute these independent tasks in parallel when TS-001 is complete:
   - [TS-002 — Get manager-order lists](application/TS-002-get-order-list.md);
   - [TS-003 — Get manager order by ID](application/TS-003-get-order-by-id.md).

## Checkpoint

Both list and detail use cases return explicit Ardalis.Result outcomes, pass cancellation, expose only agreed DTOs, and have focused Application evidence.
