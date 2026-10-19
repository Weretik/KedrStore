# Доповнення 2026-10-19 — фільтр піддерева в публічному списку товарів

**Статус:** заплановано. Історичні завершені завдання фаз 00–05 залишаються без змін.

## Залежність

Наявні `ProductCategory` з `ParentId`/`Path`, `ProductListProjection.CategoryId`, `GetPublicProductListQueryHandler` і маршрути `getPublicProducts` / `getPublicProductsByCategorySlug`. Нової міграції чи job немає.

- [ ] **TS-PC-01** У `src/Catalog/Catalog.Application/Features/Products/GetPublicList/GetPublicProductListQueryHandler.cs` замінити точний фільтр категорії на відбір усього піддерева для `categorySlug` та `categoryId`. Застосувати відбір до підрахунку, сортування й сторінки; зберегти чинні фільтри, пріоритет ID та поведінку невідомого валідного селектора. Додати вузькі автоматичні тести в `tests/IntegrationTests/Catalog/Persistence/PublicProductCategoryFilterTests.cs` для SC-PC-01–SC-PC-04 (батько/дитина/онук/сусід, обидва селектори, `ExportToSite`, інший фільтр, `pageSize = 1`, відфільтровані `totalRecords`/`totalPages`). Послідовність — Red → Green → Refactor, потім лише регресія зачепленої фічі за [правилами тестування](../../../../standards/testing-rules.md).
- [ ] **TS-PC-02** Описати семантику піддерева в `docs/sdd/contracts/catalog/products.openapi.yaml` і звірити наявні operation IDs, параметри, відповіді та `docs/sdd/features/catalog/public-products.md` із реалізацією. Оновити цю специфікацію фактичними доказами цільових тестів. Повний набір тестів, build і ручний/браузерний прогін виконувати лише за прямим запитом користувача.

## Контрольна точка

SC-PC-01–SC-PC-04 доведені вузькими автоматичними тестами; OpenAPI й карта реалізованої фічі відповідають коду. Поки TS-PC-01/02 відкриті, поведінка позначена як запланована.
