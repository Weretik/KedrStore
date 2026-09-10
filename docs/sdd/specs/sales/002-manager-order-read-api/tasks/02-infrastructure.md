# Phase 02 — Infrastructure

## Ordered tasks

1. Wait for [EN-002 — Define manager-order query contracts](application/EN-002-query-contracts.md).
2. Execute [TS-001 — Implement manager-order read model](infrastructure/TS-001-order-read-model.md), beginning with its PostgreSQL Red test before creating the read service.

## Checkpoint

The no-tracking read service implements the agreed Application contract, scopes counterparty results exactly, excludes internal diagnostics, and has PostgreSQL integration evidence.

No migration task is planned. Add one only if the agreed performance target and query-plan evidence require a new index.
