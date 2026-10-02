# Missing product photos — traceability

| Scenario | Rules | Design | Tasks | Test level | Verification evidence |
| --- | --- | --- | --- | --- | --- |
| SC-001 | R-001, R-002, R-004, R-005 | Domain state; CDN adapter; job | TS-001, TS-003, TS-004 | Unit + integration | Domain 12/12; probe image HEAD; job/store suites passed |
| SC-002 | R-003, R-005, R-007 | CDN adapter; read query/API | TS-003, TS-005, TS-006 | Integration + API | 404/410 classification and paged API tests passed |
| SC-003 | R-002, R-003 | CDN adapter and status state | TS-001, TS-003, TS-005 | Unit + integration | Missing/non-image content-type variants passed |
| SC-004 | R-003, R-006 | Job orchestration | TS-003, TS-004, TS-005 | Application + integration | Timeout, remote failure isolation, persistence, and list inclusion passed |
| SC-005 | R-004 | CDN adapter | TS-003 | Integration | 405/501 fallback and unconsumed response-body tests passed |
| SC-006 | R-001 | Domain state and import reconciliation | TS-001, TS-004 | Unit + application | URL reset, import reconciliation, and stale-result discard passed |
| SC-007 | R-005, R-008 | EF relationship and active joins | EN-001, TS-002, TS-004, TS-005 | PostgreSQL integration | EF constraints/migration script and active joins verified; 3 physical PostgreSQL checks deferred to CI because local Docker is unavailable |
| SC-008 | R-007, R-009 | Read query and controller | TS-005, TS-006 | Application + API | Anonymous `200`, filtered ID order, and database-only query passed |
| SC-009 | R-007 | Validator and HTTP mapping | TS-005, TS-006 | Unit + API | Paging boundaries, empty page, and `400` passed |
| SC-010 | R-010 | Host and deployment | TS-004, EN-002 | CLI/static deployment verification | Host dispatch compiled; exact create/update static test passed 1/1 |
| SC-011 | R-005 | Job selection | TS-004 | Application | Both `ExportToSite` values selected in store tests |

All scenarios have implementation evidence. SC-007's physical PostgreSQL
lifecycle tests are committed and mandatory in CI; they were skipped locally
because Docker is unavailable.
