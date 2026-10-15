# TS-003 — Забезпечити межі проєктів і assemblies

- **Покриває:** SC-002, SC-004
- **Залежить від:** TS-001
- **Точні шляхи:** `tests/ArchitectureTests`, production `.csproj` файли
- **Рівень тесту:** architecture

## Робота

- [x] Перетворити `ArchitectureTests` на xUnit-тестовий проєкт.
- [x] Перевірити напрям project references для Domain, Application, API, Infrastructure, hosts і тестових проєктів.
- [x] Перевірити залежності скомпільованих типів Domain і Application.
- [x] Перевірити ізоляцію бізнес-модулів із явно описаними дозволеними винятками.
- [x] Заборонити references із спільних проєктів BuildingBlocks до бізнес-модулів.
- [x] Додати повідомлення про збої з назвами порушень, придатні для виправлення.

## Свідчення TDD

- Red: поточний проєкт не містив test SDK, а `dotnet test` не виконував architecture тестів.
- Зелена фаза й рефакторинг: прибрано невикористане посилання на проєкт `Identity.Api -> Identity.Infrastructure`, адаптер XML-to-JSON перенесено до Catalog для усунення `BuildingBlocks.Infrastructure -> Catalog.Application`, а класифікацію хоста уточнено.
- Регресія: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-build --configuration Release` пройшов 10 тестів.

## Контрольна точка

ArchitectureTests виконуються в `dotnet test KedrStore.sln`, а всі закодовані правила відповідають поточній задокументованій архітектурі.
