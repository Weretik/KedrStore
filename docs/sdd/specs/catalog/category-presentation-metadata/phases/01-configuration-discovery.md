# Phase 01 — Configuration discovery

**Status:** draft

## Outcome

Define one backend configuration model for category presentation metadata, derived from the frozen storefront constants and translation snapshots rather than re-invented values.

## Work

- Map every current frontend category entry: stable category ID/slug, short UA label, short RU label and display position.
- Decide the configuration shape under `Catalog:Categories` (typed options, root/group entries and explicit defaults).
- Define which metadata is root-specific and how unknown/new 1C categories fall back safely.
- Define the one backend-owned Cosmos category: stable numeric ID, short UA/RU names and its root-level sort order.

## Acceptance criteria

- [ ] Each current frontend constant has an unambiguous backend configuration equivalent.
- [ ] Configuration uses stable IDs, not localized display text, as matching keys.
- [ ] Cosmos virtual-category metadata and stable ID are approved.

## Verification

- Manual comparison with `catalog-category-slugs.constants.ts.md` and `translate.md`.

## Source rules

- `Name` remains the original name returned by 1C and is not translated.
- `ShortNameUk` comes from the storefront constants snapshot.
- `ShortNameRu` comes from the storefront translations snapshot.
