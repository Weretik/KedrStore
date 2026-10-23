# Catalog — product-list projection: data

**Source:** [Catalog snapshot](../../../../src/Catalog/Catalog.Infrastructure/Migrations/CatalogDbContextModelSnapshot.cs) and [ProductListProjectionConfiguration](../../../../src/Catalog/Catalog.Infrastructure/Configurations/ProductListProjectionConfiguration.cs), checked 2026-10-21. [Back to database map](README.md) · [projection feature](../../features/catalog/stock-projection.md).

`ProductListProjections` has PK `ProductId` and stores denormalized list fields: `CategoryId`, `CategorySlug`, localized names, photo, `InStock`, `ExportToSite`, `IsNew`, `IsSale`, and other read fields. `ProductId` and `CategoryId` semantically associate the row with a product and category, but the snapshot has no physical FKs to `Products` or `ProductCategories`. Therefore, the physical relationship ER diagram for this table is empty.
