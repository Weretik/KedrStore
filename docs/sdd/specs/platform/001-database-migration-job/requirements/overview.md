# Database migration job — overview and scope

## Problem

Cloud Run revision `api-00054-4kq` terminated before listening on port 8080 because API startup executed EF migrations. The Identity migration source no longer matched the database history: PostgreSQL already contained `AspNetRoles`, while EF attempted to run a replacement initial migration.

## Required outcome

- The API process only serves HTTP; it does not call `Database.MigrateAsync()` during startup.
- A dedicated root job explicitly applies pending EF migrations and exits successfully only after all contexts complete.
- Production migration history remains append-only and compatible with the deployed records `20260118230934_InitialAppIdentityDbContext` and `20260507141045_AddRefreshSessionStore`.
- A deployment stops before API rollout when the migration job fails.

## Acceptance criteria

1. Starting `Host.Api` with a reachable database does not invoke any `IDatabaseMigrator`.
2. `Host.Jobs --job=migrate` runs each registered `IDatabaseMigrator` once, serially, and reports the failing context if one fails.
3. Re-running a successful migration job is idempotent: it makes no schema changes and exits with code 0.
4. The current production Identity tables are not re-created and no migration history is manually fabricated.
5. Cloud Run deploy runs the migration job successfully before it deploys the API revision.
