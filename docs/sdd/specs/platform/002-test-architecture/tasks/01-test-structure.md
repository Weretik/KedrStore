# TS-001 — Реорганізувати тести й виправити відповідальність проєктів

- **Покриває:** SC-001, SC-002
- **Залежить від:** немає
- **Точні шляхи:** `tests/UnitTests`, `tests/IntegrationTests`
- **Рівень тесту:** unit та integration регресія

## Робота

- [x] Перенести тести до папок модулів/відповідальності й відповідних namespaces.
- [x] Поділити набір тестів job синхронізації Sales на цільові класи зі спільною підтримкою.
- [x] Перенести тести Infrastructure і host з `UnitTests`.
- [x] Прибрати references на EF Core InMemory, Infrastructure та Host з `UnitTests`.
- [x] Видалити `UnitTest1.cs`.

## Виняток із test-first і свідчення

- Причина: завдання зберігає наявну поведінку, змінюючи відповідальність і структуру.
- Початковий стан до зміни: `dotnet test KedrStore.sln --no-restore --verbosity minimal` пройшов 65 модульних і 22 інтеграційні тести 2026-09-09; ArchitectureTests не виконалися.
- Регресія після зміни: `dotnet test tests/UnitTests/UnitTests.csproj --no-build --configuration Release` пройшов 41 тест; `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-build --configuration Release` пройшов 44 тести й пропустив PostgreSQL-тест через Docker.

## Контрольна точка

Усі наявні тести компілюються й проходять у своїх цільових проєктах, а UnitTests не залежить від зовнішніх production-шарів.
