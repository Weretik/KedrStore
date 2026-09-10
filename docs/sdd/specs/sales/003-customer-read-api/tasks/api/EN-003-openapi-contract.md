# EN-003 — Publish customer-read OpenAPI contract

- **Task ID:** EN-003
- **Enables:** SC-001, SC-002, SC-003, SC-004
- **Depends on:** EN-001
- **Exact paths:** `docs/sdd/specs/sales/003-customer-read-api/contracts/api-contract.md`, `docs/sdd/contracts/sales/customer-read.openapi.yaml`, `docs/sdd/contracts/openapi.yaml`
- **Test level:** API contract

## Why this is an enabler

The versioned contract fixes the public and personal-data boundary before transport implementation.

## Work

- [x] Mark the human contract agreed with final routes, operation IDs, parameters, schemas, examples, anonymous access, and errors.
- [x] Create `customer-read.openapi.yaml` for list/detail operations.
- [x] Add aggregate `$ref` entries without duplicating operations or schemas.
- [x] Describe paging/search bounds, IDs, nullable/contact fields, price rules, empty collections, and every response.
- [x] Add focused static contract assertions in a dedicated customer API test.

## Test-first exception and replacement verification

- **Reason:** OpenAPI is the test oracle preceding transport behavior.
- **Replacement check:** parse YAML and run static contract assertions.
- **Result:** Redocly validated the feature contract with only the advisory `info-license` warning; `CustomerReadContractTests` passed 1/1 and aggregate/runtime references passed in API regression.

## Checkpoint

Every customer-read operation resolves from aggregate OpenAPI and describes SC-001–SC-004.
