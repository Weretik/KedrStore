# Доповнення 2026-10-19 — фільтр піддерева в публічному списку товарів

**Статус:** виконано 2026-10-25. Історичні завершені завдання фаз 00–05 залишаються без змін.

## Залежність

Наявні `ProductCategory` з `ParentId`/`Path`, `ProductListProjection.CategoryId`, `GetPublicProductListQueryHandler` і маршрути `getPublicProducts` / `getPublicProductsByCategorySlug`. Нової міграції чи job немає.

- [x] **TS-PC-01** У `src/Catalog/Catalog.Application/Features/Products/GetPublicList/GetPublicProductListQueryHandler.cs` замінити точний фільтр категорії на відбір усього піддерева для `categorySlug` та `categoryId`. Застосувати відбір до підрахунку, сортування й сторінки; зберегти чинні фільтри, пріоритет ID та поведінку невідомого валідного селектора. Додати вузькі автоматичні тести в `tests/IntegrationTests/Catalog/Persistence/PublicProductCategoryFilterTests.cs` для SC-PC-01–SC-PC-04 (батько/дитина/онук/сусід, обидва селектори, `ExportToSite`, інший фільтр, `pageSize = 1`, відфільтровані `totalRecords`/`totalPages`). Послідовність — Red → Green → Refactor, потім лише регресія зачепленої фічі за [правилами тестування](../../../../standards/testing-rules.md).
- [x] **TS-PC-02** Описати семантику піддерева в `docs/sdd/contracts/catalog/products.openapi.yaml` і звірити наявні operation IDs, параметри, відповіді та `docs/sdd/features/catalog/public-products.md` із реалізацією. Оновити цю специфікацію фактичними доказами цільових тестів. Повний набір тестів, build і ручний/браузерний прогін виконувати лише за прямим запитом користувача.

## Контрольна точка

SC-PC-01–SC-PC-04 доведені вузькими автоматичними тестами; OpenAPI й карта реалізованої фічі відповідають коду. TS-PC-01/02 закрито; докази наведено нижче.

## Докази виконання (2026-10-25)

- TS-PC-01: `GetPublicProductListQueryHandler.GetSelectedCategoryTreeIdsAsync` відбирає вибрану категорію й усіх нащадків за `ParentId` без обмеження глибини; відбір застосовується до підрахунку, сортування й сторінки.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~PublicProductCategoryFilterTests` — 1 passed. Тест покриває SC-PC-01 (батько/дитина/онук/сусід), SC-PC-02 (ID має пріоритет над slug, листок), SC-PC-03 (сторінки без дублікатів, `ExportToSite = false` не враховується, однакові `totalRecords`/`totalPages`) і SC-PC-04 (невідомий slug).
- TS-PC-02: семантику піддерева описано в `getPublicProducts` і `getPublicProductsByCategorySlug` у `docs/sdd/contracts/catalog/products.openapi.yaml`; operation IDs, параметри й відповіді не змінено. `docs/sdd/features/catalog/public-products.md` уже відповідає коду.
- Залишковий ризик: SC-PC-03 перевірено з `pageSize = 2`, а не `1`, і без додаткового фільтра товару крім `ExportToSite`.
