# Cosmos 1C root import

**Module:** catalog  
**Status:** implemented  
**Owner:** backend team  
**Created:** 2026-07-27  
**Related:** Catalog 1C import jobs

## Goal

As a catalog administrator, I want the `Космос` 1C root to be included in catalog imports so that its categories, products, stock, prices and list projection are current on the site.

## Scope

- In scope: configure the string root `Космос`; include it in full synchronization; support its non-numeric root value in category import.
- Out of scope: new API endpoints, database schema changes and manual category hierarchy for Cosmos.
- Assumptions: 1C returns numeric category and product identifiers inside the `Космос` root responses.
- Dependencies: all four existing 1C catalog SOAP operations accept `RootCategoryId` as a string.

## Scenarios

1. Given `OneC:CosmosRootCategoryId` is `Космос`, when `--job=full` runs, then categories, details, stock and prices are imported for that root and the final projection includes its products.
2. Given a category name exists in multiple roots, when Cosmos product details are imported, then category resolution uses only Cosmos categories.

## Contract and compatibility

- API contract document: not needed because no HTTP contract changes.
- Existing consumers and compatibility: existing root IDs and CLI arguments remain unchanged; full synchronization gains a third root.
- Authorization and idempotency: not applicable to CLI jobs.

## Implementation phases

| Phase | Status | Required? | Outcome |
| --- | --- | --- | --- |
| Application | implemented | yes | String-root category import and root-scoped product category mapping. |
| Host configuration | implemented | yes | Cosmos root is configured in API and jobs hosts. |
| Documentation | not needed | no | Existing root list is illustrative; full-job source defines the executable order. |

## Files

- Modify: catalog 1C options, category/product-detail jobs, mapping/specification, full job and host configuration.
- Do not change: public API contracts, database schema and SOAP generated client.

## Acceptance criteria

- [x] `Космос` is sent as the root value to all four catalog import jobs within `full`.
- [x] A non-numeric root no longer causes category import parsing failure.
- [x] Full import rebuilds the projection after all three roots finish.
- [x] Category matching for product details is scoped to the imported root.

## Verification

- `dotnet restore KedrStore.sln`: passed, with pre-existing package vulnerability warnings.
- `dotnet build KedrStore.sln --no-restore`: passed, 0 errors and 9 warnings.
- `dotnet test KedrStore.sln --no-build --no-restore`: could not pass: `SlugTests.MapProduct_Should_Clean_Slugs` fails independently of this change; integration tests cannot connect to PostgreSQL at `127.0.0.1:5432`.

## Cosmos virtual category

- `OneC:CosmosCategoryId` is configured as `920000` in both hosts.
- The category job detects the Cosmos root, creates or updates this local category and returns without calling the unavailable categories SOAP operation.
- Product-details import assigns this category ID to every product returned for the Cosmos root, without resolving its 1C category path.
- The job stops with an explicit error if ID `920000` is already owned by a different root.

## Risks and open questions

- [ ] Validate against 1C that `GetCategories(Космос)` returns every category referenced by `GetProductDetails(Космос)`.
- [ ] A targeted run must provide `--rootId=Космос`; full import is preferred for a coherent refresh.

## Change log

- 2026-07-27 — Added Cosmos root to catalog 1C full synchronization.
