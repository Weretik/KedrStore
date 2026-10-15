# Етап 03 — Application

**Статус:** завершено й перевірено

- [x] T013 Оновити `src/Catalog/Catalog.Application/Integrations/OneC/Jobs/SyncOneCCategoryJob.cs`, щоб визначати й застосовувати затверджені метадані після побудови остаточної ієрархії, обчислювати рівні та зберігати захист від видалення за порожньої відповіді. **Залежності:** T009, T011.
- [x] T014 Зберегти й розширити шлях віртуальної категорії Cosmos у `SyncOneCCategoryJob.cs`, щоб застосовувати затверджені метадані без виклику `GetCategories`; перевірити, що `src/Catalog/Catalog.Application/Integrations/OneC/Jobs/SyncOneCProductDetailsJob.cs` і надалі безпосередньо призначає `CosmosCategoryId`. **Залежності:** T010, T011.
- [x] T015 Оновити проєкцію читання категорій без відстеження у `src/Catalog/Catalog.Application/Features/Category/GetList/`, щоб будувати детерміноване дерево з пріоритетом `SortOrder` і включати метадані лише відповідно до погодженої межі споживача. **Залежності:** T003, T011.
- [x] T016 [P] Додати тести імпорту й query-handler у `tests/UnitTests/`, зокрема fixtures фурнітури, дверей, fallback для невідомої категорії та Cosmos. **Залежності:** T013–T015.
- [x] T017 Перевірити токени скасування, використання репозиторіїв, відсутність відстеження для запитів і поведінку `Ardalis.Result` у змінених потоках. **Залежності:** T013–T016.

## Контрольна точка

Імпорт є детермінованим і безпечним щодо побічних ефектів; Cosmos не викликає недоступну операцію; читання категорій не записує стан.

## Перевірка (2026-07-29)

- `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~CategoryPresentationApplicationTests` — успішно: 5/5. Покриває налаштовану фурнітуру, двері, Cosmos, fallback для невідомої категорії й вивід дерева категорій із пріоритетом `SortOrder`.
- `SyncOneCCategoryJob` перевіряє корінь Cosmos перед будь-яким викликом `GetCategoriesAsync` і застосовує визначені для нього метадані. `SyncOneCProductDetailsJob` і надалі безпосередньо пов'язує товари Cosmos з `CosmosCategoryId`.
- Шлях імпорту передає отриманий токен скасування всім викликам репозиторіїв і 1C, зберігає захист від видалення за порожньої відповіді та виконує запис лише через `ICatalogRepository<ProductCategory>`.
- `AllCategoriesSpec` залишається `AsNoTracking`; `GetCategoriesQuryHandler` читає лише через `ICatalogReadRepository<ProductCategory>` й повертає `Result.NotFound()` або `Result.Success(...)` без запису.
