# Трасування й готовність

| Правила | Сценарії | Задачі | Запланований доказ |
| --- | --- | --- | --- |
| R-001–R-004 | SC-001, SC-002 | EN-001, TS-003 | API/SQL pagination, search, sort, detail tests |
| R-005 | SC-003, SC-004, SC-006 | EN-001, TS-001, TS-002 | pricing, validation, changed-price tests |
| R-006–R-007 | SC-005, SC-006 | EN-001, TS-002 | idempotency, transaction, authorization tests |

**Стан:** контракт підключено до aggregate OpenAPI, новий API реалізовано в локальному коміті `b3797c1`. Покриття PostgreSQL інтеграційними тестами ще потрібне.

## Рівень перевірки за сценарієм

| Сценарій | Цільова перевірка |
| --- | --- |
| SC-001 | SQL integration/API |
| SC-002 | API |
| SC-003 | unit/integration |
| SC-004 | integration/API |
| SC-005 | transaction/API |
| SC-006 | validation/API |

Детальні задачі та контрольні точки — у [графі задач](tasks/README.md). Фактичні перевірки та їхні обмеження наведено нижче.

## Фактичний evidence реалізації (2026-10-21)

- Контракт включено до `docs/sdd/contracts/openapi.yaml`; новий controller застосовує `CanManageOrders`.
- `dotnet build src/Bootstrapper/Host.Api/Host.Api.csproj --no-restore -v:q -p:NuGetAudit=false -clp:ErrorsOnly` — успіх, 0 errors.
- PostgreSQL інтеграційний тест і `EXPLAIN` не виконано; робочу БД не змінювали. Потрібна перевірка на ізольованій тестовій БД перед розгортанням.
