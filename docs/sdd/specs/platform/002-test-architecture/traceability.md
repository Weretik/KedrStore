# Test architecture — scenario traceability

| Scenario | Rules | Test level | Technical tasks | Acceptance evidence | Status |
| --- | --- | --- | --- | --- | --- |
| SC-001 | R-002 | repository structure | TS-001 | `TestSources_AreGroupedByResponsibilityAndNamespaceMatchesPath`; no `UnitTest1.cs` | verified |
| SC-002 | R-001 | project graph and unit regression | TS-001, TS-003 | `UnitTestProject_HasNoInfrastructureHostOrEfInMemoryDependency`; 41 unit tests passed | verified |
| SC-003 | R-003, R-007 | PostgreSQL integration | EN-001, TS-002 | `TwoWorkers_SendOrderOnlyOnce` implemented and discovered; local Docker skip accepted; CI cannot skip it | verified with local exception |
| SC-004 | R-004 | architecture | TS-003 | 10 layer, module, assembly, and source-organization tests passed | verified |
| SC-005 | R-005, R-006 | workflow and regression | TS-004, TS-005 | YAML lint passed; Release build and three Cobertura collections passed locally; deploy requires `verify` | verified |

## Shared enablers

| Enabler | Enables | Reason | Verification |
| --- | --- | --- | --- |
| EN-001 | SC-003, SC-005 | temporary PostgreSQL and migration lifecycle | Testcontainers 4.15.0 fixture builds; explicit local skip without Docker; CI treats missing Docker as failure |

## Notes and justified omissions

- No Domain, Application, data-model, or public API implementation task is
  required because this feature changes verification infrastructure only.
- Test-file moves and fixture extraction use a test-first exception: behavior
  is already covered, so preservation is proved by before/after regression.
- A coverage threshold is deferred until CI produces a trustworthy baseline.
