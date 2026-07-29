# Database migration job — safety requirements

- Take and verify a Cloud SQL backup before the first production migration rollout.
- Inspect the production `__EFMigrationsHistory` and table schema before changing source migration files.
- The approved destructive reset may delete only Identity tables and the two corresponding historical rows after the fresh backup is verified. It must not change Catalog or Sales data or history.
- Do not rename, squash, delete, or regenerate applied Catalog or Sales migrations.
- The migration job must use the same secret-provided connection string as the API, never log it, and stop at the first failed context.
- Run at most one migration job at a time for a database. Cloud Run Job retry/re-execution must be safe because EF migrations are idempotent after the approved reset is complete.
