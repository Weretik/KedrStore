# Аудит відповідальностей коду — Catalog після 005

- **Дата:** 2026-10-30.
- **Обсяг:** модуль Catalog після [специфікації 005](../README.md): Application (списки товарів, категорії, імпорт 1С), Domain, Infrastructure, API.
- **Правила:** [організація backend-коду](../../../../standards/backend-rules.md), [шари](../../../../architecture/overview/layers.md).
- **Метод:** огляд найбільших файлів і папок, пошук дубльованої логіки, вузькі автоматизовані перевірки після змін.

## Знахідки й рішення

| ID | Початкова проблема | Рішення та фактичний власник | Статус і доказ |
| --- | --- | --- | --- |
| AF-001 | `GetSalesList/GetProductListQueryHandler.cs` (387 рядків) поєднував фільтри, приєднання цін за правилами клієнта, сортування, пагінацію й двічі однакову проєкцію DTO. Розбір `categorySlug` шукав числовий суфікс `-<id>`, якого після 005 у адресах немає; Sales цей параметр не передає, тож гілка була мертвою. | Фільтри — `SalesProductFilters.cs`, ціни — `SalesProductPricing.cs`, рядок і проєкція — `SalesProductRow.cs`; handler лише пагінує й сортує. Каталог для продажів фільтрує тільки за `categoryId`. | **verified**: нові `SalesProductListTests` (правило ціни категорії, фільтр ціни, піддерево, ID-сортування), 2 тести. |
| AF-002 | `EscapeLike` і `NormalizeLanguage` скопійовано в п'ять handlers списків товарів. | `Catalog.Application/Querying/LikePattern.cs` і `CatalogLanguage.cs`. | **verified**: build, unit і integration тести Catalog. |
| AF-003 | `Features/Category/CategoryProductStats.cs` містив чотири типи: модель, порт, читач і згортку дерева. | `Features/Category/Stats/{CategoryStats,ICategoryStatsReader,CategoryStatsReader,CategoryStatsRollup}.cs`. | **verified**: build, `CategoryPresentationApplicationTests`, `ImportSyncTests`. |

## Рішення залишити

`ProductCategory.cs` (254 рядки) і `Product.cs` — цілісні агрегати з інваріантами. Jobs імпорту 1С уже розділені в 005 (`Sync/`: лічильники, захист від масового видалення, мапа назв, ручна ієрархія, переклади); кожен job має одну фазу. `Options/CategoryPresentationResolver.cs` містить логіку, а не налаштування; перенесення змінило б простір імен у кількох модулях без користі, тому лишено.

## Перевірка й межі

`dotnet test tests/UnitTests/UnitTests.csproj`: 152 passed. `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 10 passed. `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter Catalog`: passed (5 skipped без PostgreSQL). PostgreSQL, міграції й jobs 1С не запускались.
