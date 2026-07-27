# Phase 03 — Persistence and migration

**Status:** draft

## Outcome

Persist the new category metadata and migrate current category rows without destructive replacement.

## Work

- Update EF configuration with field lengths, nullability/default/backfill policy and any needed sort index.
- Create an explicit EF Core migration.
- Backfill existing `ShortNameUk` and `ShortNameRu` from `Name` only as a temporary fallback for unconfigured categories; configured values come from the approved snapshots.
- Ensure existing queries continue to read current columns until phase 05 moves consumers additively.

## Acceptance criteria

- [ ] Migration applies to a populated database without losing category rows.
- [ ] Rollback/down migration is feasible within project conventions.

## Verification

- Migration generation and application against a disposable PostgreSQL database.
