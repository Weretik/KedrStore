# Manager order read API — task graph

Scenario definitions are in [order-reading requirements](../requirements/order-reading.md). EN-001–EN-003 and TS-001–TS-006 are complete; detailed evidence is recorded in each task file.

## Phase orchestrators

1. [Phase 00 — Readiness](00-readiness.md)
2. [Phase 01 — Domain](01-domain.md)
3. [Phase 02 — Infrastructure](02-infrastructure.md)
4. [Phase 03 — Application](03-application.md)
5. [Phase 04 — API](04-api.md)
6. [Phase 05 — Verification](05-verification.md)

Phase numbers organize task ownership. Actual execution follows `Depends on` in this order:

```text
EN-001
├── EN-002 → TS-001 ─┬→ TS-002 ─┐
│                    └→ TS-003 ─┤
└── EN-003 ─────────────────────┴→ TS-004 → TS-005 → TS-006
```

After EN-001, EN-002 and EN-003 may run in parallel. After TS-001, TS-002 and TS-003 may run in parallel. TS-004 starts only when both application queries and the OpenAPI contract are complete.

Within every behavior-implementing `TS-*`, execute work in this mandatory order:

```text
write focused test → run and confirm expected Red → implement minimum behavior
→ run and confirm Green → refactor → run regression → record evidence
```

## Selected tasks

| ID | Responsibility | Scenarios | Depends on | File |
| --- | --- | --- | --- | --- |
| EN-001 | Resolve product and API decisions | SC-001–SC-005 | — | [task](readiness/EN-001-contract-decisions.md) |
| EN-002 | Define Application query contracts | SC-001–SC-004 | EN-001 | [task](application/EN-002-query-contracts.md) |
| TS-001 | Implement the no-tracking order read model | SC-001–SC-004 | EN-002 | [task](infrastructure/TS-001-order-read-model.md) |
| TS-002 | Implement list and counterparty-scoped queries | SC-001, SC-003 | TS-001 | [task](application/TS-002-get-order-list.md) |
| TS-003 | Implement order-detail query | SC-002, SC-004 | TS-001 | [task](application/TS-003-get-order-by-id.md) |
| EN-003 | Publish and validate the OpenAPI contract | SC-001–SC-005 | EN-001 | [task](api/EN-003-openapi-contract.md) |
| TS-004 | Add endpoints and agreed HTTP behavior | SC-001–SC-005 | TS-002, TS-003, EN-003 | [task](api/TS-004-order-read-endpoints.md) |
| TS-005 | Verify API behavior and OpenAPI conformance | SC-001–SC-005 | TS-004 | [task](api/TS-005-order-read-api-tests.md) |
| TS-006 | Run regression and close delivery evidence | SC-001–SC-005 | TS-005 | [task](verification/TS-006-verification.md) |

Implementation follows dependencies scenario by scenario. Every behavior task records Red, Green, refactor, and regression results according to the repository testing standard.
