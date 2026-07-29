# Phase 00 — Readiness and production baseline

- [ ] T001 Export and record the production `__EFMigrationsHistory` for Catalog, Identity, and Sales without sensitive values. **Dependencies:** none.
- [ ] T002 Verify the production Identity table definitions against the historical migrations `20260118230934_InitialAppIdentityDbContext` and `20260507141045_AddRefreshSessionStore`. **Dependencies:** T001.
- [ ] T003 Create and verify a Cloud SQL backup; record backup identifier and restore owner without credentials. **Dependencies:** T001.
- [ ] T004 Agree the explicit migrator context order and the Cloud Run Job name/permissions. **Dependencies:** T001, T002.

## Checkpoint

Do not restore migration files or execute a migration against production until T001-T004 are complete.
