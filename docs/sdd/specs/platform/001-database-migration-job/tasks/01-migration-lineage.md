# Phase 01 — Migration lineage

- [ ] T005 Restore the historical Identity migration files and matching snapshot from Git history under `src/Identity/Identity.Infrastructure/Migrations/`, preserving their original IDs. **Dependencies:** T002.
- [ ] T006 Compare the restored snapshot with current `AppIdentityDbContext`; generate one reviewed forward migration for actual model differences only. **Dependencies:** T005.
- [ ] T007 Add focused migration-lineage tests that assert the deployed Identity IDs remain discoverable and the forward migration is pending only where expected. **Dependencies:** T005, T006.

## Checkpoint

Do not deploy a migration that creates existing Identity tables or mutates migration history manually.
