# Catalog — categories: data

**Source:** [Catalog snapshot](../../../../src/Catalog/Catalog.Infrastructure/Migrations/CatalogDbContextModelSnapshot.cs) and [ProductCategoryConfiguration](../../../../src/Catalog/Catalog.Infrastructure/Configurations/ProductCategoryConfiguration.cs), checked 2026-10-21. [Back to database map](README.md) · [categories feature](../../features/catalog/categories.md).

`ProductCategories` (PK `Id`) stores the category tree: `ParentId`, `Path`, `Level`, `Slug`, `Name`, `ShortNameUk`, `ShortNameRu`, `SortOrder`, and `ProductTypeIdOneC`. `ParentId` represents the parent category, but the snapshot has no physical self-referencing FK. `Products.CategoryId` has an FK to `ProductCategories.Id`; see [product data](catalog-products.md) for that relationship.
