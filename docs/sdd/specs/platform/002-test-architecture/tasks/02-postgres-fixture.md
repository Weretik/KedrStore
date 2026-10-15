# EN-001 — PostgreSQL Testcontainers fixture

- **Забезпечує:** SC-003, SC-005
- **Залежить від:** TS-001
- **Точні шляхи:** `tests/IntegrationTests/TestSupport/Database`, `tests/IntegrationTests/IntegrationTests.csproj`
- **Рівень тесту:** integration infrastructure

## Навіщо потрібна передумова

`xmin`, SQL, транзакції, обмеження й міграції не можна довести через EF Core InMemory. Для змістовних тестів цих ризиків потрібен спільний життєвий цикл справжнього provider.

## Робота

- [x] Додати Testcontainers.PostgreSql 4.15.0 без вразливого транзитивного пакета.
- [x] Запускати один тимчасовий PostgreSQL container через xUnit fixture.
- [x] Застосувати міграції Sales і надати contexts із незалежними scopes.
- [x] Пропускати тест лише за недоступної локальної кінцевої точки Docker; за `CI=true` вважати це помилкою.

## Виняток із test-first і замінна перевірка

- Причина: життєвий цикл контейнера — інфраструктурна передумова, а не спостережувана production-поведінка.
- Замінна команда/перевірка: цільовий PostgreSQL integration тест.
- Результат: fixture компілювався й дав очікуваний локальний пропуск через недоступність Docker. Це погоджений виняток локального середовища; CI з Docker вважає недоступність container infrastructure помилкою.

## Контрольна точка

Середовища з Docker запускають міграції на тимчасовому PostgreSQL; локальне середовище без Docker отримує пропуск із поясненням.
