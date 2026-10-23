# Admin product list

**Module:** catalog\
**Status:** verified\
**Created:** 2026-07-24\
**Related:** Admin frontend product-list specification

## Goal

Provide an anonymously accessible product-list API for administrators, without a language segment in the route. It returns Ukrainian and Russian product names and supports both a paged filtered list and a full-list endpoint.

## Scope

- In scope: GET /api/admin/products and GET /api/admin/products/all, bilingual names, list filters, sorting, pagination, and request validation.
- Out of scope: product editing, schema migration, authorization, and a new product weight/attributes model.

## Contract and compatibility

- GET /api/admin/products uses GetAdminProductsRequest and returns PagedResult<List<AdminProductListRowDto>>.
- GET /api/admin/products/all returns List<AdminProductListRowDto> without filtering or pagination.
- A row contains identifiers, both names, slug, photo, category, availability flags, price, stock, and quantity per pack.
- Both routes explicitly use AllowAnonymous because the host's fallback policy requires authentication.

## Layered design

- Domain: no new invariants or entity changes.
- Application: separate request/handler pair, validator, and DTO.
- Infrastructure: reads Catalog ProductListProjection and Product without tracking; no migration.
- API: AdminProductsController only binds parameters, dispatches the request, and maps Result.

## Acceptance criteria

- [x] No endpoint has a language parameter in its route.
- [x] The filtered endpoint accepts search, category, availability/sale/newness, price, sorting, and pagination parameters.
- [x] Both product names and standard list fields are returned.
- [x] FluentValidation rejects invalid page, sort, category, and price-range values.

## Verification

- Catalog.Application builds without errors or warnings.
- A full Host.Api build may require stopping a locally running host first because it locks output DLL files.

## Change history

- 2026-07-24 — Initial implementation and verification record.

## Change 2026-10-19 — server-side filters for the Admin table

**Change status:** implemented; targeted verification on 2026-10-19. The earlier criteria and evidence above describe the 2026-07-24 implementation; evidence for the new increment is below. `GET /api/admin/products` and `GET /api/admin/products/all` remain in Catalog; `/all` is not the source for the paged table.

### Rules

- **R-001.** `GET /api/admin/products` applies search and all supplied filters before calculating `totalRecords`, then performs stable server sorting and pagination. An absent filter means either flag value, `true` means only true, and `false` means only false.
- **R-002.** `inStock`, `isSale`, `isNew`, and new `exportToSite` have exact three-state query semantics (`null`/`true`/`false`); specifically, `inStock=false` includes only unavailable products. `exportToSite` already exists in the projection and response, so no database migration is required.
- **R-003.** `searchTerm` searches existing Ukrainian and Russian search fields, which contain ID and name. `page` starts at `1`; `pageSize` ranges `1..100`; invalid parameters return the established `400` validation error.
- **R-004.** Add product ID as the final sort key for equal field values so products are not duplicated or skipped across pages. The table offers server sorting only for API-supported fields; other columns do not simulate sorting within the loaded page.

### Acceptance scenarios

- **SC-001 (R-001, R-002).** Given products with both values for every flag; when the client separately requests `false` for stock, sale, newness, or site export; then every result has exactly `false` and `totalRecords` counts all matches before `Skip/Take`.
- **SC-002 (R-001, R-003).** Given a Ukrainian name, Russian name, or ID match outside the first page; when the client searches and moves between pages; then search spans the full server list, `pagedInfo` reflects the filtered set, and an empty page has `value=[]`.
- **SC-003 (R-003, R-004).** When the client sends an invalid page, size, or search string, it receives `400`; sorting remains reproducible by ID when the primary sort value ties.

### Contract, design, and boundaries

An optional query parameter `exportToSite: boolean` was added to `getAdminProducts` in the [versioned OpenAPI](../../../contracts/catalog/products.openapi.yaml). Names, response DTO, `PagedResult`, error codes, and current access model are unchanged. For MUI, convert `page` from zero-based to one-based, pass `pageSize` unchanged, and map `totalRecords` to `rowCount`; this defines the contract boundary, not frontend implementation. Do not pass arbitrary MUI JSON filter models through the API: the consumer maps supported columns to named parameters. Do not confuse `ExportToSite` with photo availability.

Filtering runs on `ProductListProjection` before `LongCountAsync` and pagination; joining `Product` preserves existing row fields. Do not change Domain, database schema, import, or `/all`. Verification is limited to targeted automated tests for the changed feature under [testing rules](../../../standards/testing-rules.md); do not run the full suite without a separate user request.

### Tasks and traceability

| Task | Scenarios | Dependency | Minimum evidence |
| --- | --- | --- | --- |
| TS-001 — contract | SC-001–SC-003 | none | OpenAPI describes `exportToSite`, `false`, paging, and errors |
| TS-002 — filtering and ordering | SC-001–SC-003 | TS-001 | Targeted handler tests for `false`, search, count, and stable ordering |
| TS-003 — API and feature description | SC-001–SC-003 | TS-002 | Targeted API test for parameters/errors; update `docs/sdd/features/catalog/admin-products.md` |

**Compatibility:** the new parameter is optional, and the `inStock=false` fix changes only incorrect previous behavior. **Change history:** 2026-10-19 — implemented server-side `exportToSite`, exact `inStock=false`, and stable ordering; updated OpenAPI and the [feature map](../../../features/catalog/admin-products.md).

**Evidence:** targeted `MissingProductPhotosApiTests.AdminList_FiltersFalseBeforePaging_AndDetailsIncludeBothNamesAndFlags` verified `inStock=false`, `exportToSite=false`, `totalRecords` before paging, and exact ID; the focused API test class passed `6/6` on 2026-10-19. `ILIKE` search and all sort combinations were not separately tested against PostgreSQL; the full suite was not run under the scope rule.
