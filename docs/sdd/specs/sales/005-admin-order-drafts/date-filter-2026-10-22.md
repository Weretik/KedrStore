# Зміна 2026-10-22 — діапазон дати створення чернеток

**Статус:** SDD, код і OpenAPI не змінено. Додаток до [005 Admin drafts](README.md); попередні R-001–R-006, SC-001–SC-007 і виконані задачі залишаються історією. Спільний інкремент із frontend: `order-workspace-2026-10-22`.

## Вимоги й сценарії

- **R-007.** `getAdminOrderDrafts` приймає необов’язкові `createdFrom` і `createdTo` як `YYYY-MM-DD`; чернетки фільтруються за незмінною `CreatedAtUtc`, не за `UpdatedAtUtc`. Обидві межі включно за `Europe/Kyiv`, одна межа допустима; обернений/невалідний діапазон дає 400. SQL використовує напіввідкриті UTC-межі.
- **R-008.** Діапазон поєднується з пошуком та `counterpartyId`, `totalRecords` рахується до пагінації. Наявний порядок за `UpdatedAtUtc` лишається типовим, якщо користувач не змінив sort. DTO, модель даних і механізм видалення чернетки не змінюються; нова міграція не потрібна.

| ID | Given / When / Then |
| --- | --- |
| **SC-008** | Given чернетку створено в обраному діапазоні й змінено пізніше поза ним; When Admin задає дати; Then чернетка входить до вибірки за датою створення й count відповідає умовам. |
| **SC-009** | Given кілька чернеток різних клієнтів на межах діапазону; When Admin поєднує дати, пошук і `counterpartyId`; Then сторінка, count та стабільний порядок відповідають усім умовам. |
| **SC-010** | Given формат чи порядок меж некоректні; When Admin запитує список; Then отримує 400 без зміни даних. |

## Контракт, дизайн і задачі

Канонічна операція — `GET /api/admin/order-workspace/drafts`, `operationId: getAdminOrderDrafts` у `docs/sdd/contracts/sales/admin-order-drafts.openapi.yaml`. Додати `createdFrom?: string(date)`/`createdTo?: string(date)` до provider OpenAPI; response лишається `DraftPage`. Чинний `[AllowAnonymous]` у workspace controller зберігається. Після provider revision frontend синхронізує snapshot штатним workflow.

`AdminOrderWorkspaceController` передає діапазон у `GetWorkspaceDraftsQuery`; `WorkspaceDraftReader` накладає UTC-умови на `CreatedAtUtc` до `LongCountAsync`, `Skip/Take`. Межі й validation мають бути спільно узгоджені з [фільтром замовлень](../004-admin-order-workspace/date-filter-2026-10-22.md), без дублювання двох різних правил timezone.

| Шлях | Стан | Відповідальність |
| --- | --- | --- |
| `docs/sdd/contracts/sales/admin-order-drafts.openapi.yaml` | наявний | provider параметри |
| `src/Sales/Sales.Api/Controllers/AdminOrderWorkspaceController.cs` | наявний | binding |
| `src/Sales/Sales.Application/Features/Orders/AdminWorkspace/Drafts/GetList/` | наявний | валідація/передача |
| `src/Sales/Sales.Infrastructure/Orders/Admin/Read/WorkspaceDraftReader.cs` | наявний | фільтр, count, пагінація |
| `tests/IntegrationTests/Sales/Api/` | наявний каталог | вузькі API/SQL тести |
| `docs/sdd/features/sales/admin-order-workspace.md` | наявний | звірка після реалізації |

| Задача | Залежить від | Робота | Доказ/checkpoint |
| --- | --- | --- | --- |
| **EN-002** | 004/EN-002 | Узгодити draft OpenAPI з однаковими межами дат | contract lint, приклад редагування поза діапазоном |
| **TS-005** | EN-002 | Реалізувати query/reader фільтр | Red → Green: API/SQL тести створення vs оновлення, AND, count, 400 |
| **TS-006** | TS-005 | Звірити feature-карту й цільову регресію | traceability/evidence |

| Правила | Сценарії | Задачі | Рівень |
| --- | --- | --- | --- |
| R-007–R-008 | SC-008–SC-010 | EN-002, TS-005–TS-006 | contract/API/SQL integration |

**Готовність:** вибір поля `CreatedAtUtc`, діапазону та `Europe/Kyiv` прийнято; реалізацію ще не розпочато.
