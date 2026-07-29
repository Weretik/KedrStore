# Phase 00 — Readiness and production baseline

- [x] T001 Export and record the production `__EFMigrationsHistory` for Catalog, Identity, and Sales without sensitive values. **Dependencies:** none.
- [x] T002 Verify the production Identity table definitions against the historical migrations `20260118230934_InitialAppIdentityDbContext` and `20260507141045_AddRefreshSessionStore`. **Dependencies:** T001.
- [ ] T003 Create and verify a Cloud SQL backup; record backup identifier and restore owner without credentials. **Dependencies:** T001.
- [ ] T004 Agree the explicit migrator context order and the Cloud Run Job name/permissions. **Dependencies:** T001, T002.

## Checkpoint

Do not restore migration files or execute a migration against production until T001-T004 are complete.

## Baseline record (2026-07-29)

- Production history contains `20260118230828_InitialCatalogDbContext`, `20260428115404_Add Translate`, `20260118230934_InitialAppIdentityDbContext`, and `20260507141045_AddRefreshSessionStore`.
- Identity tables are present. `AspNetRoles.Id` is `integer`, confirming the old int-key Identity baseline and the incompatibility with the current Guid model.
- The owner approved the destructive reset of non-critical Identity data. T003 remains open until a fresh pre-reset backup is created and verified; T004 remains open until hosting IAM is verified.
