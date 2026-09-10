# EN-003 — Publish manager-order read OpenAPI contract

- **Task ID:** EN-003
- **Enables:** SC-001, SC-002, SC-003, SC-004, SC-005
- **Depends on:** EN-001
- **Exact paths:** `docs/sdd/specs/sales/002-manager-order-read-api/contracts/api-contract.md`, `docs/sdd/contracts/sales/manager-order-read.openapi.yaml`, `docs/sdd/contracts/openapi.yaml`
- **Test level:** API contract

## Why this is an enabler

The versioned contract fixes the HTTP boundary before controllers and supplies the source for frontend generation and conformance tests.

## Work

- [x] Mark the human contract agreed with final routes, methods, operation IDs, parameters, schemas, examples, security, and errors.
- [x] Create `manager-order-read.openapi.yaml` for every agreed operation.
- [x] Add one aggregate `$ref` per operation without duplicating schemas.
- [x] Describe paging/filter bounds, empty collections, IDs, decimals, timestamps, nullable values, and every response.
- [x] Add focused static contract assertions in `AdminOrderApiTests` or a dedicated contract test.

## Test-first exception and replacement verification

- **Reason:** OpenAPI is the test oracle preceding transport behavior.
- **Replacement check:** parse YAML and run static contract assertions.
- **Result:** completed 2026-09-10; `ManagerOrderReadContractTests` passed and `npx --yes @redocly/cli lint docs/sdd/contracts/sales/manager-order-read.openapi.yaml` reported a valid API description with one non-blocking repository-style license warning.
- **Aggregate lint note:** `npx --yes @redocly/cli lint docs/sdd/contracts/openapi.yaml` still reports pre-existing `security-defined` errors from `sales/admin-order-one-c-sync.openapi.yaml` and existing repository warnings. Focused aggregate `$ref` assertions and runtime route checks pass for this feature.

## Checkpoint

Every agreed operation resolves from aggregate OpenAPI and fully describes SC-001–SC-005.
