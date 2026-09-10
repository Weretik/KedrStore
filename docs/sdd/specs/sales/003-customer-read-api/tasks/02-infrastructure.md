# Phase 02 — Infrastructure

## Ordered tasks

1. Wait for [EN-002 — Define customer query contracts](application/EN-002-query-contracts.md).
2. Execute [TS-001 — Implement customer read model](infrastructure/TS-001-customer-read-model.md), beginning with its PostgreSQL Red test before creating the read service.

## Checkpoint

The no-tracking read service implements the agreed Application contract, enforces soft-delete and privacy boundaries, and has PostgreSQL integration evidence.

No migration task is planned. Add one only if the agreed performance target and query-plan evidence require a new index.
