# Database rules

## Reads and performance

Use projections and `AsNoTracking` for read use cases where appropriate. Avoid N+1 queries, unbounded list endpoints, and unnecessary materialization. Pagination and filtering for large lists must be part of the contract.

## Consistency and transactions

Follow existing transaction and concurrency boundaries. Do not add automatic retries for writes with side effects without documented idempotency. Do not weaken consistency guarantees without an explicit decision.

## EF Core, migrations, and data

EF Core model mapping belongs in Infrastructure. Each new module keeps its tables and migration history in its own schema and does not reference another module's tables; see the [module rules](module-rules.md#database-schema-per-module). Domain entities contain no persistence logic. Add a migration only when required by the feature scope; do not edit migrations that have already been applied. Check a new migration against an empty test database and describe rollback and rollout risks. The current separate `Host.Jobs --job=migrate` execution is described in [operations](../operations/data/migrations-and-seeding.md).

Preserve query semantics and database contracts. Change lookup/seed data through the existing seeding mechanism, not through hidden runtime writes.
