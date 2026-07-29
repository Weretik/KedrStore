# Category presentation metadata — category presentation

## Behavior

- On category import, the system stores the 1C-provided full category name in `Name` without substituting a presentation label.
- For every configured category, the system applies approved `ShortNameUk`, `ShortNameRu`, `SortOrder`, and `Level` deterministically.
- Root categories have level `0`; each descendant level is one greater than its parent.
- A category tree requested by the Catalog Application consumer is ordered by `SortOrder` and then by numeric category ID.
- Existing category IDs, slugs, paths, parent links, and current response fields remain compatible.

## Rules and invariants

- `Name`, `ShortNameUk`, and `ShortNameRu` represent different concepts; an import must not overwrite `Name` with either short label.
- `Level` is non-negative and agrees with the final parent/path hierarchy after import.
- Metadata matching uses the existing `(ProductTypeIdOneC, CategoryId)` pair, never localized display text alone.
- The configuration is the source of configured labels and order; the two snapshots are frozen migration inputs only.
- `ShortNameUk` and `ShortNameRu` are required, trimmed, non-blank, and at most 100 characters.
- `SortOrder` is non-negative and unique among siblings. Snapshot order is zero-based; an unconfigured category uses `Int32.MaxValue` and both of its short labels fall back to its original `Name`.

## Acceptance scenarios

1. Given a configured 1C category, when the category job imports it, then the stored `Name` equals the original 1C value and its two configured short labels, order, and level are stored independently.
2. Given the doors or hardware root and its direct presentation groups, when the tree is imported, then the root has level `0` and each direct group has level `1`.
3. Given an unconfigured 1C category, when it is imported, then the approved fallback labels and order are applied without changing its stable ID, slug, path, or parent.
