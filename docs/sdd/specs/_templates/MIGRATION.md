# Incremental migration to scenario-first SDD

Do not rewrite all historical feature documents at once. Migrate an existing
feature when its behavior changes or when the user explicitly requests a full
specification migration.

## Feature with new or changed behavior

1. Preserve existing task IDs and verification history.
2. Assign `R-*` and `SC-*` IDs to the behavior being changed. Existing unchanged
   scenarios may be assigned IDs when needed for dependencies, without rewriting
   their wording.
3. Add `traceability.md` and include all changed scenarios plus any existing
   scenarios needed to explain regression risk.
4. Create `SC-*` orchestration files only for the changed scope.
5. Give new technical tasks `TS-*` IDs and shared prerequisites `EN-*` IDs.
6. Apply Red -> Green -> Refactor to new behavior and record evidence.
7. Keep old completed phase files as historical records; do not convert their
   checkbox IDs unless the user asks for a full migration.

## Feature already in progress

Finish currently active old-format tasks without renumbering or invalidating
their evidence. Apply the new test-first protocol to remaining new behavior
when it can be done without contradicting an already accepted task. Introduce
scenario IDs and traceability at the next safe specification checkpoint.

## Full migration

For a user-authorized full migration, map every current acceptance scenario to
`SC-*`, create the complete traceability matrix, and translate only unfinished
work into `TS-*`/`EN-*` tasks. Retain completed legacy task files as linked
history or archive them through a separate reviewed change.

## Migration checkpoint

The changed behavior has stable scenario IDs, new work is traceable, existing
evidence remains intact, and the migration does not silently expand scope.
