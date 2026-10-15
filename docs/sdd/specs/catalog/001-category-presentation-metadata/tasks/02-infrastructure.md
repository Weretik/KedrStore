# Етап 02 — Infrastructure

**Статус:** завершено й перевірено

- [x] T009 Додати погоджену типізовану модель options представлення в `src/Catalog/Catalog.Application/Integrations/OneC/Options/` і прив'язати її в `src/Catalog/Catalog.Infrastructure/DependencyInjection/CatalogIntegrationOneCExtension.cs` або наявній точці компонування. **Залежності:** T002, T008.
- [x] T010 Додати затверджені однакові записи конфігурації до `src/Bootstrapper/Host.Api/appsettings.json` і `src/Bootstrapper/Host.Jobs/Host.Jobs/appsettings.json`. **Залежності:** T009.
- [x] T011 Налаштувати поля й погоджений індекс упорядкування, якщо він потрібний, у `src/Catalog/Catalog.Infrastructure/Configurations/ProductCategoryConfiguration.cs`; створити нову міграцію в `src/Catalog/Catalog.Infrastructure/Migrations/`. **Залежності:** T005, T006, T002.
- [x] T012 [P] Додати перевірки збереження/міграції в `tests/IntegrationTests/` за проєктною домовленістю тестування. **Залежності:** T011.

## Перевірка (2026-07-29)

- `dotnet ef migrations add AddCategoryPresentationMetadata --project src/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj --startup-project src/Bootstrapper/Host.Api/Host.Api.csproj --context CatalogDbContext --output-dir Migrations` — успішно завершено; EF Core згенерував міграцію `20260729060741_AddCategoryPresentationMetadata`.
- Конфігурації обох хостів містять однакові 55 записів представлення, не мають дубльованих ключів `(ProductTypeIdOneC, CategoryId)` й охоплюють кожну налаштовану ручну групу та дочірню категорію.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~CategoryPresentationPersistenceTests` — успішно: 2/2.
- Міграція додає потрібні стовпці й індекс упорядкування дерева `(ParentId, SortOrder, Id)`. Її `Up` заповнює наявні рядки значенням `Name` для обох коротких назв, порядком сортування `Int32.MaxValue` і рівнем `nlevel(Path) - 1`; `Down` видаляє додані індекс і стовпці.

## Контрольна точка

Для міграції задокументовано заповнення наявних даних і шлях відкату, хости використовують однакову затверджену конфігурацію, а жодна застосована міграція не редагується.
