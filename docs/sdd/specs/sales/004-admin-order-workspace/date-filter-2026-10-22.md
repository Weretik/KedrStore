# Зміна 2026-10-22 — діапазон дат замовлень

**Статус:** SDD, код і версійний OpenAPI ще не змінено. Додаток до [004 Admin workspace](README.md); історичні R-001–R-007, SC-001–SC-006 і докази залишаються незмінними. Спільний ID інкременту з Admin frontend: `order-workspace-2026-10-22`.

## Вимоги й приймання

- **R-008.** `getAdminWorkspaceOrders` приймає необов’язкові `createdFrom` і `createdTo` у форматі календарної дати `YYYY-MM-DD`. Межі включно за часовим поясом `Europe/Kyiv`; в SQL перетворювати на напіввідкритий UTC-інтервал `[початок createdFrom, початок дня після createdTo)`. Це коректно охоплює дні переходу літнього часу. Одна межа допустима, `createdFrom > createdTo` повертає 400.
- **R-009.** Фільтр поєднується через AND із наявними `searchTerm` та `counterpartyId`; `totalRecords` обчислюється після всіх умов і до `Skip/Take`, стабільний sort зберігається. Той самий endpoint обслуговує головну сторінку та таблицю в деталях клієнта; в останній `counterpartyId` є обов’язковим frontend-контекстом.
- **R-010.** Параметри не змінюють DTO запису, авторизаційне рішення, idempotency створення чи дані БД. Нова міграція не потрібна. Індекс пропонувати лише за результатом `EXPLAIN` на релевантному обсязі.

| ID | Given / When / Then |
| --- | --- |
| **SC-007** | Given замовлення на межах і поза діапазоном у київському часі; When Admin запитує `createdFrom`/`createdTo`; Then повертаються лише записи включених календарних днів і правильний `totalRecords`. |
| **SC-008** | Given клієнт має замовлення різних дат, а інші клієнти теж мають записи; When запит містить `counterpartyId` і діапазон; Then сторінка містить тільки записи цього клієнта в діапазоні. |
| **SC-009** | Given межі переплутані або мають невалідний формат; When Admin запитує список; Then отримує 400 без SQL-запиту з невалідними умовами. |

## Контракт, дизайн і порядок

Канонічна операція — `GET /api/admin/order-workspace/orders`, `operationId: getAdminWorkspaceOrders` у `docs/sdd/contracts/sales/admin-order-workspace.openapi.yaml` та агрегованому `docs/sdd/contracts/openapi.yaml`. Запланувати query-параметри `createdFrom?: string(date)`, `createdTo?: string(date)`; response лишається наявним `OrderPage`, помилка 400 — чинний безпечний формат. До реалізації оновити лише provider OpenAPI й contract tests, після фіксації revision передати frontend для штатного snapshot sync. Чинний контролер тимчасово `[AllowAnonymous]` за рішенням попереднього етапу; ця зміна доступу не торкається.

Запит проходить `AdminOrderWorkspaceController` → `GetWorkspaceOrdersQuery` → `WorkspaceOrderReader`. Валідацію формату/порядку меж виконати до reader; UTC-межі обчислювати в одному місці. `CreatedAtUtc` не перетворювати функцією в SQL `WHERE`, щоб зберегти можливість використання індексу. Відсутність обох меж має зберегти поточну поведінку.

| Шлях | Стан | Відповідальність |
| --- | --- | --- |
| `docs/sdd/contracts/sales/admin-order-workspace.openapi.yaml` | наявний | версійний HTTP-контракт дати |
| `src/Sales/Sales.Api/Controllers/AdminOrderWorkspaceController.cs` | наявний | query binding і 400 |
| `src/Sales/Sales.Application/Features/Orders/AdminWorkspace/Orders/GetList/` | наявний | валідація/передача умов |
| `src/Sales/Sales.Infrastructure/Orders/Admin/Read/WorkspaceOrderReader.cs` | наявний | UTC-умови, count і сторінка |
| `tests/IntegrationTests/Sales/Api/` | наявний каталог | вузькі API/SQL тести меж і pagination |
| `docs/sdd/features/sales/admin-order-workspace.md` | наявний | звірка карти після реалізації |

| Задача | Залежить від | Робота | Доказ/checkpoint |
| --- | --- | --- | --- |
| **EN-002** | — | Узгодити OpenAPI параметри та timezone `Europe/Kyiv`; зафіксувати provider revision | contract lint, точні приклади DST/400 |
| **TS-005** | EN-002 | Додати binding, validation, reader-фільтр у зазначених шляхах | Red → Green: API/SQL тести меж, AND умов, count |
| **TS-006** | TS-005 | Звірити feature-карту, контракт і цільовий regression | traceability/evidence, без повного прогону без запиту |

| Правила | Сценарії | Задачі | Рівень |
| --- | --- | --- | --- |
| R-008–R-010 | SC-007–SC-009 | EN-002, TS-005–TS-006 | contract/API/SQL integration |

**Готовність:** продуктове рішення про діапазон і часовий пояс прийнято. Немає блокера для provider контракту; реалізація не доручена цим документом.
