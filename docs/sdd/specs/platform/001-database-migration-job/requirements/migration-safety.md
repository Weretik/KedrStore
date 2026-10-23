# Database migration job — safety requirements

- Create and verify a Cloud SQL backup before the first production migration deployment.
- Inspect production `__EFMigrationsHistory` and table schemas before changing migration files in code.
- The agreed destructive reset may remove only Identity tables and the two corresponding historical rows after verifying a fresh backup. Catalog and Sales data and migration history remain unchanged.
- Do not rename, merge, delete, or regenerate applied Catalog or Sales migrations.
- The migration job must use the same secret-backed connection string as the API, never log it, and stop at the first failing context.
- Run no more than one migration job at a time for a database. Rerunning the Cloud Run Job must be safe because EF migrations are idempotent after the agreed reset is complete.
