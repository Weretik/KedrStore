# Category read API — category navigation

## Behavior

- A client requests the category collection with `lang=uk` or `lang=ru` and receives a complete forest of root categories. Each node includes its recursively ordered children.
- A client requests a category with its public `categorySlug` and receives that category, its recursively ordered descendants, and breadcrumbs from the root to the category.
- `name` is `ShortNameUk` for `uk` and `ShortNameRu` for `ru`. The original 1C `Name`, 1C root key, and technical `Path` are not public API fields.
- The frontend uses `id` as the stable category identity and `slug` to build public category URLs. It must not derive identity, order, or hierarchy from a localized name.

## Rules and invariants

- Every returned node has a positive integer `id`, a non-empty `slug` and `name`, `sortOrder >= 0`, `level >= 0`, and an array `children` (empty for leaves).
- `parentId` is `null` only for a root. For a non-root node, it identifies its immediate parent in the returned tree.
- `level` is zero for roots and equals the number of edges from the root; it is backend-owned metadata, not a value calculated by clients.
- Siblings are ordered ascending by `sortOrder`; equal values are ordered ascending by `id`.
- The collection returns `200` and `[]` when no categories have been imported. A requested slug that does not exist returns `404` with no body.
- Invalid `lang` or a blank/invalid `categorySlug` returns the established `400` validation-error response. The endpoints are read-only and have no idempotency key.

## Acceptance scenarios

1. Given roots and children with mixed `SortOrder` values, when the collection is requested, then every sibling list is ordered by `SortOrder`, then `Id`, and each `level` and `parentId` describes the same hierarchy.
2. Given a category with Ukrainian and Russian short names, when the same collection is requested with `uk` and `ru`, then the node identity, hierarchy, order, and slug are unchanged while `name` is localized.
3. Given a nested category, when its slug is requested, then the response contains the node's descendants and breadcrumbs ordered root-to-current category.
4. Given an unknown slug, when a client requests the single-category endpoint, then it receives `404` and no category data.
