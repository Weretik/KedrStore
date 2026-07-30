# Category read API — overview and scope

## Goal

As a storefront or administration frontend, I want to read the current category hierarchy through a contract appropriate to my access level, so that I can render public navigation and administer catalog data without duplicating category constants.

## In scope

- Anonymous `GET /api/catalog/{lang}/categories` returning every public root and its recursive children.
- Anonymous `GET /api/catalog/{lang}/categories/by-slug/{categorySlug}` returning the requested category, its recursive descendants, and its root-to-category breadcrumbs.
- Temporarily anonymous `GET /api/categories` and `GET /api/categories/{id}` for the internal administration UI, returning an unlocalized flat read model with both display names and source metadata.
- A frontend DTO containing `id`, `slug`, localized `name`, nullable `parentId`, `sortOrder`, `level`, and `children`; the single-category DTO also contains `breadcrumbs`.
- Deterministic sibling ordering by `sortOrder`, then numeric `id`; `lang` selects the localized short display name.
- Versioned OpenAPI contract, aggregate-contract reference, controller/handler tests, and frontend integration-guide update.

## Out of scope

- Category create, update, delete, reordering, or import changes.
- Product filtering, product-count aggregation, images, descriptions, SEO metadata, or category-specific promotions.
- Pagination or lazy-loading of the navigation tree; the existing catalog hierarchy is delivered as one finite tree.
- Frontend implementation or removal of its existing constants before consumer cutover.

## Open questions

- None. Public and admin routes, access policies, DTOs, ordering, and error semantics are agreed in [the API contract](../contracts/api-contract.md).

Do not describe EF, handlers, controllers, migrations, or file structure here.
