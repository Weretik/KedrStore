# Customer read API — scenario traceability

| Scenario | Rules | Test level | Domain | Application | Infrastructure | API/contract | Acceptance evidence | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SC-001 | R-001, R-002, R-005, R-006 | Application, API, integration | — | EN-002, TS-002 | TS-001 | EN-003, TS-004, TS-005 | `SalesCustomerReadModelTests.GetListAsync_ReturnsOnlyActiveCustomersOrderedByNameAndId`; `GetListAsync_AppliesPagingAndReturnsEmptyPagePastTheEnd`; `AdminCustomerApiTests.GetList_ReturnsCompactPageAndUsesPagingDefaults` | verified |
| SC-002 | R-001, R-003, R-004, R-005 | Application, API, integration | — | EN-002, TS-003 | TS-001 | EN-003, TS-004, TS-005 | `SalesCustomerReadModelTests.GetByIdAsync_ReturnsActiveCustomerAndOrderedCategoryPriceTypes`; `AdminCustomerApiTests.GetById_ReturnsApprovedDetailWithoutPrivateOrOrderData` | verified |
| SC-003 | R-002, R-005 | Application, API | — | EN-002, TS-003 | TS-001 | EN-003, TS-004, TS-005 | `SalesCustomerReadModelTests.GetByIdAsync_ReturnsNotFoundForUnknownAndSoftDeletedCustomers`; `AdminCustomerApiTests.GetById_ReturnsNotFoundForUnavailableCustomer` | verified |
| SC-004 | R-005 | API | — | — | — | EN-003, TS-004, TS-005 | `AdminCustomerApiTests.ReadRoutes_ReturnSuccessWithoutAuthentication` verifies both list and detail | verified |

## Shared enablers

| Enabler | Enables | Reason | Verification |
| --- | --- | --- | --- |
| EN-001 | SC-001–SC-004 | Agree route design, anonymous access, response privacy boundary, list semantics, and deleted-customer behavior | Specification readiness checklist and anonymous-access amendment are complete |
| EN-002 | SC-001–SC-003 | Establish privacy-bounded query projections without coupling Application to EF | Application build and focused handler fixtures pass |
| EN-003 | SC-001–SC-004 | Establish the versioned HTTP oracle before transport implementation | Redocly validation, aggregate `$ref`, static assertions, and runtime route checks pass |

## Notes and justified omissions

- No Domain task is planned because all scenarios query existing aggregates without changing domain behavior.
- No migration is planned; it must be reconsidered only if agreed list behavior has a measured indexing need.
