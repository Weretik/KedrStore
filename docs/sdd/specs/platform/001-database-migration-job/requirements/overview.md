# Database migration job — overview and scope

## Problem

Cloud Run revision `api-00054-4kq` exited before listening on port 8080 because API startup ran EF migrations. Identity migration code no longer matched database history: PostgreSQL already had `AspNetRoles`, while EF tried to apply a replacement initial migration.

## Required outcome

- The API process only serves HTTP; it does not call `Database.MigrateAsync()` on startup.
- A separate root job explicitly applies the expected EF migrations and succeeds only after all contexts complete.
- The agreed destructive reset removes only noncritical Identity data, then establishes `20260529142739_InitialAppIdentityDbContext` as the new Identity baseline.
- If the migration job fails, deployment stops before updating the API.

## Acceptance criteria

1. Starting `Host.Api` with an available database invokes no `IDatabaseMigrator`.
2. `Host.Jobs --job=migrate` runs each registered `IDatabaseMigrator` exactly once in sequence and reports the failing context.
3. A repeated successful migration job is idempotent: it does not change the schema and exits with code 0.
4. The agreed Identity reset does not delete Catalog or Sales data; afterward, only the new Guid-based Identity baseline is recorded.
5. Cloud Run deployment runs the migration job successfully before publishing the new API revision.
