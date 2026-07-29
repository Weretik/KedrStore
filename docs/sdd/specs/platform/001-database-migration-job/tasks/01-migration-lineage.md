# Phase 01 — Migration lineage

- [x] T005 Verify `20260529142739_InitialAppIdentityDbContext` creates the complete current Guid Identity schema from an empty Identity baseline. **Dependencies:** T002.
- [x] T006 Prepare reviewed reset SQL that drops only approved Identity tables and deletes only the two obsolete Identity history rows after backup confirmation. **Dependencies:** T005.
- [x] T007 Add focused migration-lineage tests that assert the Guid initial migration is discoverable and creates the schema from an empty Identity baseline. **Dependencies:** T005.

## Checkpoint

Do not execute reset SQL until the fresh backup and foreign-key review are recorded. The reset SQL must not reference Catalog or Sales tables.

## Completion record (2026-07-29)

- `IdentityMigrationLineageTests` verifies that the current Guid initial migration exposes only `20260529142739_InitialAppIdentityDbContext` and generates the required Identity and refresh-session schema.
- The approved transactional script is in `design/identity-reset.sql.md`. It blocks on non-Identity foreign keys and does not use `CASCADE`.
