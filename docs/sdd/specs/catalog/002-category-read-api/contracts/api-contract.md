# Category read API — API contract

**Status:** agreed

The machine-readable source is [category-read.openapi.yaml](../../../../contracts/catalog/category-read.openapi.yaml), referenced by the aggregate [OpenAPI contract](../../../../contracts/openapi.yaml). It is a new additive public Catalog API.

| Operation | Route | Success | Errors | Access |
| --- | --- | --- | --- | --- |
| `getCategoryTree` | `GET /api/catalog/{lang}/categories` | `200` array of recursive `CategoryTreeNode`; `[]` when empty | `400` invalid language | Anonymous |
| `getCategoryBySlug` | `GET /api/catalog/{lang}/categories/by-slug/{categorySlug}` | `200` `CategoryDetails` | `400` invalid input; `404` unknown slug | Anonymous |
| `getAdminCategories` | `GET /api/categories` | `200` flat `AdminCategory` array; `[]` when empty | — | Anonymous, temporary internal UI surface |
| `getAdminCategoryById` | `GET /api/categories/{id}` | `200` `AdminCategory` | `400` invalid id; `404` absent id | Anonymous, temporary internal UI surface |

`lang` is exactly `uk` or `ru`. `categorySlug` is a non-empty public slug of at most 100 characters. Responses use camel case. `CategoryTreeNode` contains `id`, `slug`, locale-selected `name`, `parentId`, `sortOrder`, `level`, and recursive `children`. `CategoryDetails` adds `breadcrumbs`, ordered root-to-current category; each breadcrumb contains `id`, `slug`, and localized `name`.

All operations are safe, read-only GET operations, so no idempotency key or request body applies. The two public operations are explicitly anonymous, consistent with public product routes despite the host fallback authorization policy. The ordinary `400` validation-error array and bodyless `404` follow the existing API convention.

The admin operations are temporarily anonymous, consistent with the existing admin-products read surface. They return both localized short names, the original source `name`, and `productTypeIdOneC`, unlike the locale-specific public response. The admin list is an unpaged depth-first flattening of the tree with `sortOrder`, then `id` within each sibling set; it keeps `parentId` and `level` so the UI can reconstruct the hierarchy. A future external-exposure change must add authorization before changing this contract.

The explicit `by-slug` segment avoids a route collision with the existing `/{categorySlug}/products` endpoint. Compatibility is additive: no current route or schema changes. Clients must treat unknown additive response fields as ignorable. During rollout, deploy the API and its aggregate OpenAPI reference atomically; frontend clients must generate types from the aggregate contract rather than backend DTOs.
