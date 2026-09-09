# Test architecture — task graph

## Tasks

| ID | Responsibility | Depends on | File |
| --- | --- | --- | --- |
| TS-001 | Reorganize tests and correct project ownership | none | [01-test-structure.md](01-test-structure.md) |
| EN-001 | Add reusable PostgreSQL Testcontainers fixture | TS-001 | [02-postgres-fixture.md](02-postgres-fixture.md) |
| TS-002 | Prove real `xmin` concurrent claim behavior | EN-001 | [03-postgres-concurrency.md](03-postgres-concurrency.md) |
| TS-003 | Enforce layer, module, and test-project dependencies | TS-001 | [04-architecture-tests.md](04-architecture-tests.md) |
| TS-004 | Gate deploy and publish coverage | TS-002, TS-003 | [05-ci-and-coverage.md](05-ci-and-coverage.md) |
| TS-005 | Run and record repository verification | TS-004 | [06-verification.md](06-verification.md) |

Execute tasks in dependency order under the
[AI feature workflow](../../../_templates/ai-feature-workflow/README.md).
