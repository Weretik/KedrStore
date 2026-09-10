# TS-005 — Verify customer HTTP and contract behavior

- **Task ID:** TS-005
- **Covers:** SC-001, SC-002, SC-003, SC-004
- **Depends on:** TS-004
- **Exact paths:** `tests/IntegrationTests/Sales/Api/AdminCustomerApiTests.cs`, `tests/IntegrationTests/Platform/Api/ApiContractTests.cs`, `docs/sdd/contracts/sales/customer-read.openapi.yaml`
- **Test level:** API verification

## Verification evidence

- **Source behavior task:** TS-004 Red/Green evidence.
- **Distinct risks:** PII leakage, soft-delete leakage, serialization, anonymous-route drift, and OpenAPI drift.
- **Command/result:** after the anonymous-access amendment, the focused customer/runtime contract run passed 15/15; full regression evidence is recorded in TS-006.
- **Remaining gap:** none for the accepted scope; soft-delete behavior is proven at PostgreSQL projection level and HTTP not-found mapping is tested for both unknown/deleted cases.

## Work

- [x] Test list success/empty/invalid input and detail success/unknown/deleted customer.
- [x] Test successful unauthenticated access for every route.
- [x] Assert the approved DTO omits disallowed PII, `IdentityUserId`, audit/deletion state, and Domain entities.
- [x] Verify identifier, nullable contact, price-rule, and paging serialization against OpenAPI.
- [x] Verify runtime routes match versioned and aggregate OpenAPI.
- [x] Verify reads neither call 1C/Identity nor mutate Sales persistence.

## Checkpoint

Focused tests prove every scenario, privacy boundary, and runtime contract.
