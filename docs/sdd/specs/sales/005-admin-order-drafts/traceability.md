# Трасування й готовність

| Правила | Сценарії | Задачі | Запланований доказ |
| --- | --- | --- | --- |
| R-001–R-002 | SC-001, SC-002 | EN-001, TS-001, TS-002 | domain/EF/validation tests |
| R-003, R-006 | SC-002, SC-004 | EN-001, TS-002 | concurrency/access/delete tests |
| R-004 | SC-003 | TS-003 | SQL search/sort/page tests, query plan |
| R-005 | SC-005 | TS-002, TS-004; 004/TS-002 | повторне використання без зміни draft |
| R-002–R-003 | SC-006 | TS-002 | GET detail при недоступній поточній ціні |
| R-001, R-006 | SC-007 | TS-002 | конкурентний POST з одним ключем |

**Стан:** окремі таблиці, API та міграцію реалізовано в локальному коміті `b3797c1`; міграцію до БД не застосовано. Покриття PostgreSQL інтеграційними тестами ще потрібне.

## Рівень перевірки за сценарієм

| Сценарій | Цільова перевірка |
| --- | --- |
| SC-001 | domain/EF/API |
| SC-002 | domain/API |
| SC-003 | SQL integration/API |
| SC-004 | EF/API |
| SC-005 | integration/API |
| SC-006 | integration/API |
| SC-007 | PostgreSQL concurrency/API |

Детальні задачі та контрольні точки — у [графі задач](tasks/README.md). Фактичні перевірки та їхні обмеження наведено нижче.

## Фактичний evidence реалізації (2026-10-21)

- Адитивну `AddAdminOrderDrafts` migration і snapshot згенеровано; migration не застосовували до БД.
- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter FullyQualifiedName~OrderDraftTests -v:q -p:NuGetAudit=false` — 5 passed.
- `dotnet build src/Bootstrapper/Host.Api/Host.Api.csproj --no-restore -v:q -p:NuGetAudit=false -clp:ErrorsOnly` — успіх, 0 errors.
- PostgreSQL CRUD/concurrency та `EXPLAIN` не перевірено; потрібна ізольована тестова БД перед розгортанням.
