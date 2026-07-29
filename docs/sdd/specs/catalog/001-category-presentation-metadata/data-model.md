# Category presentation metadata — data model

## Context

The existing `ProductCategories` table stores 1C ownership, original category name, slug, parent ID, and ltree path. It needs display-oriented data that is separate from the 1C source name and can be consumed without a storefront-maintained mapping.

## Model

```text
ProductCategory / ProductCategories
├── Id: ProductCategoryId (existing stable numeric ID)
├── ProductTypeIdOneC: string (existing root ownership)
├── Name: string (existing original 1C full name)
├── Slug: string (existing stable URL slug)
├── ParentId: ProductCategoryId? (existing hierarchy relation)
├── Path: CategoryPath / ltree (existing hierarchy path)
├── ShortNameUk: string (new configured Ukrainian display label)
├── ShortNameRu: string (new configured Russian display label)
├── SortOrder: int (new deterministic sibling/root order)
└── Level: int (new depth derived from final hierarchy)
```

## Invariants and integrity

- `Name` is required, has the existing maximum length of 100, and remains the original 1C name.
- `ShortNameUk` and `ShortNameRu` are required after migration backfill, trimmed and non-blank, with a maximum length of 100.
- `SortOrder` is deterministic, non-negative, and unique per `ParentId` sibling set (including the root sibling set). An unconfigured category uses `Int32.MaxValue`; a numeric category ID breaks any resulting tie.
- `Level >= 0`; a root has level `0`, and the value matches the hierarchy derived from `Path`/`ParentId`.
- Add an index only if the agreed category-tree query requires database ordering beyond the current in-memory tree construction; index shape follows the agreed read access pattern.
