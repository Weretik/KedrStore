# EN-001 — Agree customer-read behavior

- **Task ID:** EN-001
- **Enables:** SC-001, SC-002, SC-003, SC-004
- **Depends on:** none
- **Exact paths:** `docs/sdd/specs/sales/003-customer-read-api/requirements/overview.md`, `docs/sdd/specs/sales/003-customer-read-api/requirements/customer-reading.md`, `docs/sdd/specs/sales/003-customer-read-api/contracts/api-contract.md`, `docs/sdd/specs/sales/003-customer-read-api/data-model.md`, `docs/sdd/specs/sales/003-customer-read-api/traceability.md`, `docs/sdd/specs/sales/003-customer-read-api/checklist/spec-readiness.md`
- **Test level:** documentation review

## Why this is an enabler

Routes, access mode, personal-data fields, price rules, list semantics, and soft-delete behavior change public behavior and privacy boundaries.

## Work

- [x] Resolve every customer clarification or move it explicitly out of scope.
- [x] Agree routes, operation IDs, parameters, result fields, examples, status codes, and anonymous access.
- [x] Agree pagination, ordering, lack of search/extra filters, empty list, and unknown/deleted-customer behavior.
- [x] Include phone in list; include email/default/category price types in detail; exclude `IdentityUserId` and order data.
- [x] Update rules/scenarios while preserving their stable IDs.
- [x] Mark the human contract agreed and complete specification readiness.

## Test-first exception and replacement verification

- **Reason:** this task records product/privacy decisions and changes no executable behavior.
- **Replacement check:** verify that every previously open product/privacy decision has an agreed contract outcome and traceability mapping.
- **Result:** completed 2026-09-10; decisions are recorded in requirements and the agreed API contract.

## Checkpoint

The specification defines an explicit least-privilege response and access boundary.
