# Category in the Admin product table — 2026-10-19

**Goal.** The manager sees each product's Ukrainian category name, selects a category from a readable hierarchy, and gets server-side filtering and ordering across the full result set before pagination.

## Rules and scenarios

- **R-CAT-01.** `GET /api/admin/products` and `GET /api/admin/products/missing-photos` accept optional `categoryId > 0`. Selecting a category includes it and all descendants, regardless of depth. Without `categoryId`, all categories remain available.
- **R-CAT-02.** Both Admin list rows include `categoryName` from `ProductCategory.ShortNameUk`; a missing reference returns an empty string, which the client displays as an unavailable name. The contract change is additive.
- **R-CAT-03.** `CategoryAsc` and `CategoryDesc` sort by Ukrainian category name, with product ID as the final stable key. `totalRecords` is calculated after filtering and before `Skip/Take`.
- **SC-CAT-01.** When a root category is selected, products from it and descendants at level three and beyond appear in `value`, and `totalRecords` counts all matches before paging.
- **SC-CAT-02.** When sorting categories ascending/descending, the server returns global order and stable product ordering within a category.
- **SC-CAT-03.** When a category is selected in «Без фото» mode, the server applies the same filter to the products with unavailable photos.

## Design and tasks

Catalog remains the owner of filters and categories. `ProductListProjection.CategoryId` joins to `ProductCategory`; the tree is built from `ParentId` and is not truncated at two levels. No new tables or migrations. OpenAPI at `docs/sdd/contracts/catalog/products.openapi.yaml` is the source of truth. New `ProductSort` values are additive; public operations that consume this shared enum must also define category ordering rather than silently substituting ID order.

| Task | Scenario | Evidence |
| --- | --- | --- |
| TS-CAT-01 — contract and DTO | SC-CAT-01–03 | OpenAPI and DTO contain categoryId/categoryName/CategoryAsc/CategoryDesc |
| TS-CAT-02 — server filtering and ordering | SC-CAT-01–03 | Targeted tests for deep trees, counts, and sorting |
| TS-CAT-03 — feature documentation | SC-CAT-01–03 | Updated Admin products feature map and contract |

**Verification boundary:** automated tests for the changed feature only; do not run browser checks, the full suite, or the database without separate authorization.

## Static boundary audit (2026-10-19)

Reviewed only `GetAdminProductListQueryHandler`, `GetMissingProductPhotosQueryHandler`, `GetPublicProductListQueryHandler`, and `AdminProductCategoryIds`. Confirmed violation: the previous Admin filter included only two descendant levels; it was replaced by a full `ParentId` traversal with a visited-ID set. List handlers own filtering/order/pagination and DTO projection; the helper only returns tree IDs. The Public handler already had a full traversal, so its filter was unchanged. PostgreSQL SQL translation of the new `CategoryAsc/Desc` still needs a safe test that does not modify the database; an InMemory API test does not prove it.
