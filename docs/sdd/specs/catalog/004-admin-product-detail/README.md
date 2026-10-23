# 004 — Admin product details

**Module:** Catalog. **Status:** Implemented; targeted verification completed. **Date:** 2026-10-19.

## Goal and boundaries

Provide a read-only API for one product for the Admin details page. The existing [public endpoint by slug](../../../contracts/catalog/products.openapi.yaml) returns a name in only one language and omits some Admin flags; the new route returns both names, scheme, and state without depending on whether the product is available in the public catalog. Editing, deletion, changes to 1C import, database schema, and image URL checks are out of scope.

## Rules

- **R-001.** `GET /api/admin/products/by-slug/{productSlug}` accepts a non-empty product slug no longer than `255` characters, matching the existing `ProductListProjection.ProductSlug` limit. For an existing active product it returns one object without pagination; a missing product returns `404`, and an empty or oversized value returns the established `400` validation error if the request reaches the controller. The static `by-slug` segment avoids conflicts with `/all` and `/missing-photos`.
- **R-002.** The response contains `id`, `nameUk`, `nameRu`, `productSlug`, `photo`, `scheme`, `categoryId`, `categoryName`, `inStock`, `isSale`, `isNew`, `exportToSite`, `price`, `stock`, `quantityInPack`. Flags and price agree with the current Admin list row. `scheme` returns the saved URL; media availability is determined by the client, and the API makes no CDN request.
- **R-003.** Admin data is available even when `exportToSite=false`. `productSlug` lets frontend create a public catalog link only when `exportToSite=true`; the API returns no environment-dependent absolute URL and does not claim the public page is guaranteed to be available.
- **R-004.** The public operation `GET /api/catalog/{lang}/product/{productSlug}` and its DTO remain compatible. The new route has the current access model of other Admin product operations (`AllowAnonymous`); access restrictions require a separate decision for all Admin operations.

## Acceptance scenarios

- **SC-001 (R-001, R-002).** Given a product with different Ukrainian and Russian names, a scheme, price, and active flags; when the client requests its slug; then it receives `200` with all listed fields and exact `true`/`false` values.
- **SC-002 (R-002, R-003).** Given a product not exported to the site and with an unavailable scheme file; when the client requests details; then it receives `200`, `exportToSite=false`, and the saved `scheme` URL. Photo or scheme availability is not checked during the HTTP request. The client hides the scheme section after a load error.
- **SC-003 (R-001).** A missing product slug returns `404`; an empty or over-limit slug returns the established validation error if the request matches the route. A missing segment receives the standard routing `404`.
- **SC-004 (R-004).** After adding the Admin endpoint, the existing public endpoint by slug continues returning its current contract regardless of the new Admin detail model.

## HTTP contract and compatibility

| Question | Decision |
| --- | --- |
| Route and method | `GET /api/admin/products/by-slug/{productSlug}`, read-only |
| operationId | `getAdminProductBySlug` |
| Parameter | `productSlug`: non-empty string, max `255` characters, no truncation |
| Response | `200`: `AdminProductDetails`, fields from R-002; `price` nullable, `scheme` is saved URL |
| Errors | `400` for invalid slug that matched routing; `404` for missing/inactive product or missing segment; other failures follow shared API rules |
| Access | Current explicit anonymous access on `AdminProductsController`; no new authorization scheme in this increment |
| Idempotency | Safe GET; no key required |

The operation and schema were added to the [module OpenAPI](../../../contracts/catalog/products.openapi.yaml), and its route `$ref` to [aggregated OpenAPI](../../../contracts/openapi.yaml). The contract is additive; current list DTOs and public product DTO do not change. A public URL example using the slug belongs to frontend site configuration/routing, not the backend response.

## Design and data

`AdminProductsController` passes the slug to a separate CQRS request; Application reads `Product`, `ProductListProjection`, and category without tracking. The projection provides bilingual names and flags, `Product` provides the scheme and quantity per pack, and category provides the name. Required columns already exist; no schema change, migration, import change, or new external integration is needed. The projection retains products that are not exported to the site. The [Admin products map](../../../features/catalog/admin-products.md), its file tree, and flow diagram were updated.

## Tasks and traceability

| Task | Scenarios | Dependency | Minimum evidence |
| --- | --- | --- | --- |
| TS-001 — machine-readable contract | SC-001–SC-004 | none | OpenAPI: route, DTO, errors, access, `$ref` |
| TS-002 — Application query | SC-001–SC-003 | TS-001 | Targeted tests for both languages, flags, scheme URL, and `404` by slug |
| TS-003 — HTTP and feature map | SC-001–SC-004 | TS-002 | Targeted API tests for `200`/`400`/`404` and unchanged public contract; map updated |

**Evidence as of 2026-10-19:** `MissingProductPhotosApiTests.AdminList_FiltersFalseBeforePaging_AndDetailsIncludeBothNamesAndFlags` returned `200` by slug for a product with `exportToSite=false`, and checked different `nameUk`/`nameRu`, `isSale`, `inStock`, `exportToSite`, and the `scheme` URL; the focused API test class passed `6/6`. Slug validation unit tests are included in the targeted `7/7`. A separate HTTP test for `404`, oversized slug, or actual media availability was not run; this is the limit of the evidence, not a claim about CDN state. The full project suite was not run under the [verification rules](../../../standards/testing-rules.md). **Remaining risk:** Admin operations currently allow anonymous access in code; this increment does not change that.
