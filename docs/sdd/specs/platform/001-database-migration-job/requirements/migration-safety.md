# Database migration job — safety requirements

- Take and verify a Cloud SQL backup before the first production migration rollout.
- Inspect the production `__EFMigrationsHistory` and table schema before changing source migration files.
- Preserve deployed migration IDs and `Up`/`Down` semantics. Do not rename, squash, delete, or regenerate a migration that is already recorded in production.
- Generate a new forward migration only after the restored snapshot is compared with the current `AppIdentityDbContext` model.
- The migration job must use the same secret-provided connection string as the API, never log it, and stop at the first failed context.
- Run at most one migration job at a time for a database. Cloud Run Job retry/re-execution must be safe because EF migrations are idempotent when history is consistent.
