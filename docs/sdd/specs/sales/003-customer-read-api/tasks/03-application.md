# Phase 03 — Application

## Ordered tasks

1. After EN-001, execute [EN-002 — Define customer query contracts](application/EN-002-query-contracts.md).
2. Wait for Infrastructure TS-001.
3. Execute these independent tasks in parallel when TS-001 is complete:
   - [TS-002 — Get active-customer list](application/TS-002-get-customer-list.md);
   - [TS-003 — Get customer by counterparty ID](application/TS-003-get-customer-by-id.md).

## Checkpoint

Both list and detail use cases return explicit Ardalis.Result outcomes, pass cancellation, expose only approved fields, and have focused Application evidence.
