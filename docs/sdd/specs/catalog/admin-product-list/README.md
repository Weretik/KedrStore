# Admin product list

**Module:** catalog  
**Status:** verified  
**Created:** 2026-07-24  
**Related:** admin frontend product-list specification

## Goal

Provide an anonymous admin-facing product-list API without a language route segment. It returns both Ukrainian and Russian product names and supports a pageable filtered list plus a full-list endpoint.

## Scope

- In scope: GET /api/admin/products and GET /api/admin/products/all, bilingual names, list filters, sorting, paging and query validation.
- Out of scope: product editing, schema migration, authorization and a new product weight/attribute model.

## Contract and compatibility

- GET /api/admin/products uses GetAdminProductsRequest and returns PagedResult<List<AdminProductListRowDto>>.
- GET /api/admin/products/all returns List<AdminProductListRowDto> without filtering or paging.
- The row contains identifiers, both names, slug, photo, category, stock flags, price, stock and quantity-in-pack.
- Both routes are explicitly AllowAnonymous because the host fallback policy requires authentication.

## Design by layer

- Domain: no new invariant or entity change.
- Application: dedicated query/handler pair, validator and DTO.
- Infrastructure: queries Catalog ProductListProjection and Product as no-tracking read sources; no migration.
- API: AdminProductsController only binds, dispatches and maps Result.

## Acceptance criteria

- [x] Neither endpoint contains a language path parameter.
- [x] Filtered endpoint accepts search, category, stock/sale/new, price, sorting and paging parameters.
- [x] Both product names and standard list fields are returned.
- [x] Invalid page, sort, category and price ranges are rejected by FluentValidation.

## Verification

- Catalog.Application builds with zero errors and warnings.
- Full Host.Api build may require stopping the locally running host first because it locks output DLL files.

## Change log

- 2026-07-24 — Initial implementation and verification record.
