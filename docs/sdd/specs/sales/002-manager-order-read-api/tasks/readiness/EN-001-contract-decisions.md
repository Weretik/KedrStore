# EN-001 — Agree manager-order read behavior

- **Task ID:** EN-001
- **Enables:** SC-001, SC-002, SC-003, SC-004, SC-005
- **Depends on:** none
- **Exact paths:** `docs/sdd/specs/sales/002-manager-order-read-api/requirements/overview.md`, `docs/sdd/specs/sales/002-manager-order-read-api/requirements/order-reading.md`, `docs/sdd/specs/sales/002-manager-order-read-api/contracts/api-contract.md`, `docs/sdd/specs/sales/002-manager-order-read-api/data-model.md`, `docs/sdd/specs/sales/002-manager-order-read-api/traceability.md`, `docs/sdd/specs/sales/002-manager-order-read-api/checklist/spec-readiness.md`
- **Test level:** documentation review

## Why this is an enabler

Routes, authorization, visible fields, list semantics, and deleted-counterparty history change public behavior. A meaningful implementation test cannot be written until these decisions are explicit.

## Work

- [x] Resolve every manager-order clarification or move it explicitly out of scope.
- [x] Agree list/detail routes, operation IDs, parameters, result fields, examples, status codes, and security.
- [x] Agree pagination, deterministic ordering, filters, empty-list behavior, and unknown/deleted-counterparty behavior.
- [x] Confirm that authorized Manager/Admin callers can see all orders without row-level assignment filtering.
- [x] Update rules/scenarios while preserving their stable IDs.
- [x] Mark the human contract agreed and complete the specification-readiness checklist.

## Test-first exception and replacement verification

- **Reason:** this task records product decisions and changes no executable behavior.
- **Replacement check:** verify that every previously open product decision has an agreed contract outcome and traceability mapping.
- **Result:** completed 2026-09-10; decisions are recorded in requirements and the agreed API contract.

## Checkpoint

The specification can produce request/response contracts and failing behavioral tests without invented behavior.
