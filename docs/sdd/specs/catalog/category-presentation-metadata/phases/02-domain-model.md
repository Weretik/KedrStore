# Phase 02 — Domain model

**Status:** draft

## Outcome

Extend `ProductCategory` to retain its original 1C name and own short localized display names, ordering and nesting metadata without replacing current stable identifiers or slugs.

## Design

- Retain `Name` as the original full name received from 1C; do not translate it.
- Add `ShortNameUk`, `ShortNameRu`, `SortOrder` and non-negative `Level`.
- Put length, requiredness and `Level >= 0` invariants in the aggregate.
- Keep `Path` and `ParentId` as structural source of truth; validate that `Level` agrees with the computed hierarchy where data is imported.

## Acceptance criteria

- [ ] New fields have explicit domain invariants and update behavior.
- [ ] Existing code using category ID, slug, `Path` and `ParentId` remains compatible.

## Verification

- Unit tests for creation/update validation and level calculation.
