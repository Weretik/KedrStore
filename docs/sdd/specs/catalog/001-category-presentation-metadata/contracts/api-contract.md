# Category presentation metadata — API and integration contract

**Status:** no HTTP contract in scope

## Existing integration contract

The existing 1C SOAP client contract is unchanged. Cosmos remains a backend-owned virtual category: category import must not call `GetCategories` for the configured Cosmos root, and product-details import assigns `OneC:CosmosCategoryId` to every Cosmos product.

## HTTP consumer decision

The repository contains an application `GetCategoriesQuery` and `CategoryTreeDto`, but no API controller or route dispatches that query. HTTP exposure is not in scope for this feature: the Catalog Application read model is the consuming boundary.

`CategoryTreeDto` preserves its existing `Id`, `Name`, and `Children` fields and additively exposes `ShortNameUk`, `ShortNameRu`, `SortOrder`, and `Level`. Children are ordered by `SortOrder`, then numeric `Id`.

No route, operation ID, authorization policy, OpenAPI document, or API task is created. A later feature that introduces HTTP exposure must first agree an additive contract that preserves `id`, `name`, and `children` semantics.
