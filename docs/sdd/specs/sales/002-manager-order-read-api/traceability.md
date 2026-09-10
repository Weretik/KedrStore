# Manager order read API — scenario traceability

| Scenario | Rules | Test level | Domain | Application | Infrastructure | API/contract | Acceptance evidence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SC-001 | R-001, R-005, R-006 | Application, API, integration | — | EN-002, TS-002 | TS-001 | EN-003, TS-004, TS-005 | `SalesOrderReadModelTests.GetListAsync_ProjectsPersistedOrdersNewestFirst`; `ManagerOrderReadApiTests.GetList_ReturnsCompactPageAndPassesExactFilter` | verified |
| SC-002 | R-002, R-004, R-005 | Application, API, integration | — | EN-002, TS-003 | TS-001 | EN-003, TS-004, TS-005 | `SalesOrderReadModelTests.GetByIdAsync_ReturnsSafeDetailForSoftDeletedCounterpartyWithoutChangingSync`; `ManagerOrderReadApiTests.GetById_ReturnsApprovedDetailAndDoesNotCallOneC` | verified |
| SC-003 | R-003, R-005 | Application, API, integration | — | EN-002, TS-002 | TS-001 | EN-003, TS-004, TS-005 | `SalesOrderReadModelTests.GetListAsync_AppliesExactCounterpartyScopeAndPaging`; `ManagerOrderReadApiTests.GetList_ReturnsEmptyPageForUnknownCounterparty` | verified |
| SC-004 | R-002, R-005 | Application, API | — | EN-002, TS-003 | TS-001 | EN-003, TS-004, TS-005 | `GetOrderByIdQueryHandlerTests.Handle_PreservesNotFoundAndReadFailure`; `ManagerOrderReadApiTests.GetById_ReturnsNotFoundForUnknownOrder` | verified |
| SC-005 | R-005 | API | — | — | — | EN-003, TS-004, TS-005 | `ManagerOrderReadApiTests.ReadRoutes_ReturnUnauthorizedWithoutAuthentication`; `ManagerOrderReadApiTests.ReadRoutes_ReturnForbiddenForAuthenticatedUser` | verified |

## Shared enablers

| Enabler | Enables | Reason | Verification |
| --- | --- | --- | --- |
| EN-001 | SC-001–SC-005 | Agree routes, DTO boundary, authorization, list semantics, and customer-history visibility | Specification readiness checklist is complete before implementation |
| EN-002 | SC-001–SC-004 | Establish query projections without coupling Application to EF | Application build and focused test fixtures pass |
| EN-003 | SC-001–SC-005 | Establish the versioned HTTP oracle before transport implementation | Redocly validation, aggregate `$ref`, static assertions, and runtime route checks pass |

## Notes and justified omissions

- No Domain task is planned because all scenarios query existing aggregates without changing domain behavior.
- No migration is planned; its omission must be re-evaluated only if an agreed performance target requires a new index.
