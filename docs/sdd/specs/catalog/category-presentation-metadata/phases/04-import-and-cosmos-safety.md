# Phase 04 — Import and Cosmos safety

**Status:** draft

## Outcome

The category import applies configured presentation metadata and all catalog jobs remain safe for roots with and without a categories response.

## Work

- Preserve the original 1C category name in `Name`; do not translate or overwrite it with display labels.
- Merge typed configuration by stable category ID/root to set short names and `SortOrder`.
- Compute `Level` from the final parent/path tree: roots 0, direct children 1, and so on.
- Preserve existing empty-response deletion guard.
- Do not call `GetCategories` for Cosmos. Ensure the configured virtual Cosmos category exists, then assign its ID to every product returned by `GetProductDetails(Космос)`.
- Add targeted import tests for doors, hardware and Cosmos.

## Acceptance criteria

- [ ] Import deterministically applies metadata for configured categories.
- [ ] Unknown categories receive documented fallback labels/order and a correct level.
- [ ] Cosmos imports do not call the unavailable categories operation, do not delete other roots and assign every imported Cosmos product to the virtual Cosmos category.

## Verification

- Unit tests with 1C DTO fixtures and integration tests for persistence.
