# Catalog — проєкція списку товарів: дані

**Джерело:** [Catalog snapshot](../../../../src/Catalog/Catalog.Infrastructure/Migrations/CatalogDbContextModelSnapshot.cs) і [ProductListProjectionConfiguration](../../../../src/Catalog/Catalog.Infrastructure/Configurations/ProductListProjectionConfiguration.cs), перевірено 2026-10-21. [Назад до карти БД](README.md) · [фіча проєкції](../../features/catalog/stock-projection.md).

`ProductListProjections` має PK `ProductId` і зберігає денормалізовані поля для списків: `CategoryId`, `CategorySlug`, локалізовані назви, фото, `InStock`, `ExportToSite`, `IsNew`, `IsSale` та інші поля читання. `ProductId` та `CategoryId` пов'язують запис за змістом із товаром і категорією, але фізичних FK до `Products` і `ProductCategories` у snapshot немає. Через це ER-діаграма фізичних зв'язків для цієї таблиці порожня.
