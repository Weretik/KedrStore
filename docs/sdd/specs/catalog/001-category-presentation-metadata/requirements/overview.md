# Category presentation metadata — overview and scope

## Goal

As a catalog administrator, I want category presentation metadata to be owned and imported by the backend, so that category navigation can use one source of truth instead of a manually maintained storefront constants file.

## In scope

- Retain the original full 1C category name independently from localized short display labels.
- Configure and persist Ukrainian and Russian short labels, deterministic display order, and hierarchy level.
- Apply the approved metadata during category import for hardware, doors, and the backend-owned Cosmos category.
- Make the existing category-tree read model capable of returning the required metadata when an agreed consumer contract requires it.
- Preserve safe Cosmos product import without a 1C `GetCategories` call.

## Out of scope

- Modifying the storefront repository or removing its constants file.
- Changing public routes, category/product identifiers, existing slugs, or generated 1C SOAP client code.
- Translating, normalizing, or replacing the original 1C `Name` field.
- Creating an admin UI for metadata management.

## Agreed delivery decisions

- Metadata matches by the existing `(ProductTypeIdOneC, CategoryId)` pair; it never matches localized text or a generated slug.
- An unconfigured category preserves its 1C `Name`, receives that name as both short labels, and is ordered after configured siblings; the full deterministic policy is in `tasks/00-readiness.md`.
- No public HTTP category-tree response is in scope. The Catalog Application category-tree read model is the consuming boundary.
