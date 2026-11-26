# ADR-0015: The product lists read from one denormalized read table

- Status: accepted (records a decision already in the code)
- Date: 2026-11-17 (owner's request after the DDD and CQRS audit)
- Owner: Виталий (project owner)

## Context

The public catalogue, its search, the sales catalogue and the admin product list show the same row for every product: Ukrainian and Russian names, slug, photo, category slug, stock flag, sale and new flags, «show on the site», and the retail price (`Catalog:Pricing:RetailPriceTypeId`: price type 12, type 11 «Розница халтмаг 30%» since 2026-11-26, spec catalog/010). These values live in five tables of the Catalog module (`Products`, `ProductTranslations`, `ProductPrices`, `Categories`, product media). Each list filters by category and flags, searches the names with `ILIKE` on `pg_trgm` indexes, sorts by name or price and pages in SQL.

The rest of the backend is CQRS level 1: commands change aggregates, queries read the same tables straight into DTOs ([module rules, Domain model](../../standards/module-rules.md#domain-model-ddd)). The checklist allows level 2, a separate read table, only for a concrete list that a level-1 query cannot serve well.

## Alternatives considered

- Level 1: one query joining the five tables for every page. Rejected for these lists: the search, the filters and the price sort run over the joined rows of every product, so each page joins and filters the whole catalogue; the trigram indexes cannot cover a search across two tables (`Products.Name` and `ProductTranslations.Name`).
- A PostgreSQL materialized view: rejected; it is rebuilt as a whole, cannot be refreshed row by row, and its definition would live in a migration instead of the code that already knows the retail price type and the hidden categories.
- A separate read database or search engine (level 3): rejected; one PostgreSQL carries the load and the owner has not agreed to data that lags behind for minutes.

## Decision

- The table `ProductListProjections` (Catalog, which still uses the `public` schema) holds one row per visible product with every list field, and indexes on category, category slug, flags, retail price and both search texts (`gin_trgm_ops`).
- The row type `ProductListProjection` is a read model, not an aggregate: it lives in `Catalog.Application/Features/Products/Shared/`, has no business rules, and only the rebuild writes it. The query handlers of the product lists read it without tracking.
- `IProductListProjectionRebuilder` (`Application/Abstractions/Persistence`), implemented by `ProductListProjectionRebuilder` in Infrastructure, rebuilds the table in one transaction under a PostgreSQL advisory lock, writing only rows whose values changed (database rules 2.4); a product media change refreshes only the photo column with one SQL update. It runs after every change of its sources: the 1C full, stock, price and product-detail syncs, product media changes, and the «Перебудувати список товарів» background task (`product-list`).
- Consistency: the lists may show the previous values until the rebuild that follows the change finishes (seconds after a manual media change, the end of the sync run for 1C data). Product pages and orders read the source tables, never this table, so prices in an order are always current.

## Why

- Every list page is one indexed query on one table, whatever the filters, sort and search.
- The source aggregates stay small and free of list concerns.

## Trade-offs

- A second copy of the list fields: a new source of a list field must call the rebuild, or the lists show stale data until the next sync. Reviews check this.
- No measurement was recorded when the table was introduced; the next performance review compares a list page against a level-1 join on production data and records the numbers here.

## Sources and checks

- `src/Modules/Catalog/Catalog.Application/Features/Products/Shared/ProductListProjection.cs`, `src/Modules/Catalog/Catalog.Infrastructure/Persistence/Products/ProductListProjectionRebuilder.cs`, `ProductListProjectionConfiguration.cs`.
- Tests: `tests/UnitTests/Catalog/Application/ProductListProjectionSyncTests.cs`, product list integration tests under `tests/IntegrationTests/Catalog/`.
