# Database rules

## Reads and performance

For read use cases, use projections and `AsNoTracking` where appropriate. Avoid N+1 queries, unbounded list endpoints, and unnecessary materialization. Pagination and filtering for large lists must be part of the contract.

## Consistency and transactions

Respect existing transaction and concurrency boundaries. Do not add automatic retries to writes with side effects without documented idempotency. Do not silently weaken consistency guarantees.

## EF Core, migrations, and data

EF Core mapping belongs in Infrastructure. Domain entities contain no persistence concerns. Create a migration only when it is required within the feature scope; do not edit applied migrations. Verify a new migration against an empty test database and document rollback and rollout risks.

Preserve query semantics and database contracts. Change reference/seed data through the existing seeding mechanism, not hidden runtime writes.
