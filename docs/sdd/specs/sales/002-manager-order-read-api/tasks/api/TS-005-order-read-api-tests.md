# TS-005 — Verify manager-order HTTP and contract behavior

- **Task ID:** TS-005
- **Covers:** SC-001, SC-002, SC-003, SC-004, SC-005
- **Depends on:** TS-004
- **Exact paths:** `tests/IntegrationTests/Sales/Api/AdminOrderApiTests.cs`, `tests/IntegrationTests/Platform/Api/ApiContractTests.cs`, `docs/sdd/contracts/sales/manager-order-read.openapi.yaml`
- **Test level:** API verification

## Verification evidence

- **Source behavior task:** TS-004 Red/Green evidence.
- **Distinct risks:** serialization, status codes, authorization, route collision, OpenAPI drift.
- **Command/result:** combined `AdminOrderApiTests|ManagerOrderReadApiTests|ApiContractTests|ManagerOrderReadContractTests` run passed 30/30; full solution run passed 59 Unit, 10 Architecture, and 67 Integration tests with no skips when pointed at an isolated PostgreSQL 18 schema.
- **Remaining gap:** none.

## Work

- [x] Test list success/empty/counterparty scope/invalid parameters and detail success/not-found.
- [x] Test unauthenticated and forbidden responses for every route.
- [x] Assert approved DTOs omit Domain entities, SOAP bodies, diagnostics, notification state, and retry audits.
- [x] Verify decimal, timestamp, enum, nullable, and line-array serialization against OpenAPI.
- [x] Verify runtime routes match versioned and aggregate OpenAPI.
- [x] Verify reads neither call 1C nor alter sync state.

## Checkpoint

Focused tests prove each scenario and runtime contract conformance.
