# EN-001 — PostgreSQL Testcontainers fixture

- **Enables:** SC-003, SC-005
- **Depends on:** TS-001
- **Exact paths:** `tests/IntegrationTests/TestSupport/Database`, `tests/IntegrationTests/IntegrationTests.csproj`
- **Test level:** integration infrastructure

## Why this is an enabler

`xmin`, SQL, transactions, constraints, and migrations cannot be proved by EF
Core InMemory. A reusable real-provider lifecycle is required before those
risks have meaningful tests.

## Work

- [x] Add Testcontainers.PostgreSql 4.15.0 without introducing a vulnerable transitive package.
- [x] Start one temporary PostgreSQL container through an xUnit fixture.
- [x] Apply Sales migrations and provide independently scoped contexts.
- [x] Skip only when the local Docker endpoint is unavailable; fail instead when `CI=true`.

## Test-first exception and replacement verification

- Reason: container lifecycle is prerequisite infrastructure rather than observable production behavior.
- Replacement command/check: focused PostgreSQL integration test.
- Result: the fixture compiled and produced the expected local skip because
  Docker is unavailable. This is the accepted local environment exception;
  Docker-enabled CI treats unavailable container infrastructure as a failure.

## Checkpoint

Docker-enabled environments run migrations against temporary PostgreSQL;
Docker-less local environments receive a descriptive skip.
