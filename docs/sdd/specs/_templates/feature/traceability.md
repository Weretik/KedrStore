# <feature name> — scenario traceability

Use one row per acceptance scenario. A task may cover several scenarios only
when they share one technical responsibility. Use `—` when a layer is not
required and explain non-obvious omissions below the table.

| Scenario | Rules | Test level | Domain | Application | Infrastructure | API/contract | Acceptance evidence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SC-001 | R-001 | <Domain/Application/API/Integration> | TS-001 | TS-002 | EN-001 | TS-003 | <test name or planned test> | planned |
| SC-002 | R-001 | <level> | TS-004 | TS-005 | — | TS-006 | <test name or planned test> | planned |

## Shared enablers

| Enabler | Enables | Reason | Verification |
| --- | --- | --- | --- |
| EN-001 | SC-001, SC-002 | <shared prerequisite> | <check> |

## Notes and justified omissions

- <why a layer or automated test is not required>
