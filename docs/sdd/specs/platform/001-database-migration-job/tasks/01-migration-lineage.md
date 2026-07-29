# Phase 01 — Migration lineage

- [ ] T005 Verify `20260529142739_InitialAppIdentityDbContext` creates the complete current Guid Identity schema from an empty Identity baseline. **Dependencies:** T002.
- [ ] T006 Prepare reviewed reset SQL that drops only approved Identity tables and deletes only the two obsolete Identity history rows after backup confirmation. **Dependencies:** T005.
- [ ] T007 Add focused migration-lineage tests that assert the Guid initial migration is discoverable and creates the schema from an empty Identity baseline. **Dependencies:** T005.

## Checkpoint

Do not execute reset SQL until the fresh backup and foreign-key review are recorded. The reset SQL must not reference Catalog or Sales tables.
