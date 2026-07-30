# Category read API — design: Domain

## Responsibility

No domain behavior changes. `ProductCategory` remains the owner of category identity, slug, parent relationship, localized short names, order, and level. The API projects the existing read model and must not reveal the aggregate/entity directly.

## Cross-module interaction

The Catalog API calls Catalog Application queries through Mediator. Query handlers read through `ICatalogReadRepository<ProductCategory>` with the existing no-tracking specification and return `Ardalis.Result`; they do not publish events or write state. The public single-category query resolves by slug; the admin single-category query resolves by numeric `ProductCategoryId`, both through Application specifications rather than parsing a route value.

## Risks

An import can update category presentation data while a request is reading. Each response is a point-in-time database read; no cache freshness promise is made in this feature. A malformed legacy hierarchy must not cause an infinite recursive DTO build: the application projection must detect/avoid cycles and return an expected failure with diagnostics rather than overflowing.
