# 006 — Розподіл відповідальностей Admin workspace Sales

- **Дата:** 2026-10-21.
- **Обсяг:** лише код SDD [004](../004-admin-order-workspace/README.md) і [005](../005-admin-order-drafts/README.md); HTTP, OpenAPI, модель БД та job 1C не змінюються.
- **Підстава:** [аудит 004/005](../004-admin-order-workspace/code-audit/audit.md) і [правила шарів](../../../architecture/overview/layers.md).

## Вимоги, завдання й перевірка

| Вимога | Знахідка | Завдання й ціль | Перевірка |
| --- | --- | --- | --- |
| AR-001: один Application handler на сценарій, API надсилає Mediator request | AF-001 | RM-001: дев'ять пар query/command та handler у `Sales.Application/Features/Orders/AdminWorkspace/{Orders,Drafts}/<scenario>/`; controller працює через `ISender`. | Збірка Sales.Api й layer architecture tests. |
| AR-002: Infrastructure має вузькі адаптери за роллю | AF-001 | RM-002: замість partial service — `Admin/Read/{WorkspaceOrderReader,WorkspaceDraftReader}`, `Admin/Checkout/WorkspaceOrderCreator`, `Admin/Drafts/{WorkspaceDraftWriter,WorkspaceDraftDetails}`, `Admin/Pricing/AdminWorkspacePricing`, `Admin/Idempotency/WorkspaceIdempotency`. | Компіляція DI, вузькі тести сценаріїв і PostgreSQL перевірка перед rollout. |
| AR-003: контракти не змішують незалежні потоки | AF-002 | RM-003: DTO згруповано у `Workspace{Quote,Order,Draft,Page}Contracts.cs`, `WorkspaceInputLine.cs`; порти читання, ціни й запису — окремі `IWorkspace*.cs`. | Компіляція, перевірка OpenAPI без зміни операцій. |
| AR-004: прибрати мертвий код | AF-003 | RM-004: невикористану локальну проєкцію `lines` із читання деталей вилучено; лишається SQL-запит позицій. | Збірка й відповідний read test. |

**Порядок:** RM-003 → RM-002 → RM-001; RM-004 разом із перенесенням читання. Відкат виконується групою разом із DI та controller; міграції й дані не змінюються. [Карта реалізованої фічі](../../../features/sales/admin-order-workspace.md) показує фактичні шляхи. Поточні статуси й докази залишаються в аудиті 004/005, щоб не дублювати історію.
