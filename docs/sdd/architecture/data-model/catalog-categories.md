# Catalog — категорії: дані

**Джерело:** [Catalog snapshot](../../../../src/Catalog/Catalog.Infrastructure/Migrations/CatalogDbContextModelSnapshot.cs) і [ProductCategoryConfiguration](../../../../src/Catalog/Catalog.Infrastructure/Configurations/ProductCategoryConfiguration.cs), перевірено 2026-10-21. [Назад до карти БД](README.md) · [фіча категорій](../../features/catalog/categories.md).

`ProductCategories` (PK `Id`) зберігає дерево категорій: `ParentId`, `Path`, `Level`, `Slug`, `Name`, `ShortNameUk`, `ShortNameRu`, `SortOrder`, `ProductTypeIdOneC`. `ParentId` указує на батьківську категорію за змістом, але фізичний FK до цієї ж таблиці в snapshot відсутній. `Products.CategoryId` має FK до `ProductCategories.Id`; зв'язок товарів описано в [даних товарів](catalog-products.md).
