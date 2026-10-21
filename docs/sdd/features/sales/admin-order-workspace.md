# Робоча область замовлень Admin

**Модуль:** Sales. **Споживач:** React Admin. **Стан:** код і контракт у репозиторії; міграцію не застосовано до робочої БД.

[AdminOrderWorkspaceController](../../../../src/Sales/Sales.Api/Controllers/AdminOrderWorkspaceController.cs) відкриває окремі маршрути `/api/admin/order-workspace` під політикою `CanManageOrders`. Наявний `/api/admin/orders` залишається окремим контрактом. [AdminOrderWorkspaceService](../../../../src/Sales/Sales.Infrastructure/Orders/AdminOrderWorkspaceService.cs) виконує пошук, сортування й сторінковий перегляд замовлень; [AdminWorkspacePricing](../../../../src/Sales/Sales.Infrastructure/Orders/AdminWorkspacePricing.cs) бере політику вибраного клієнта та ціни Catalog. Під час оформлення сервер перераховує суму, порівнює fingerprint і через чинний OrderCreationStore атомарно записує Order та Pending 1C sync.

Чернетки реалізовано окремою [моделлю](../../../../src/Sales/Sales.Domain/Orders/Entities/OrderDraft.cs) і [сервісом CRUD](../../../../src/Sales/Sales.Infrastructure/Orders/AdminOrderWorkspaceService.Drafts.cs). Вони не отримують номера Order і не передаються в 1C. Відповідь деталі містить актуальний серверний розрахунок або `pricingStatus=Unavailable` зі збереженими позиціями та `null` замість недоступних цін; PUT/DELETE використовують `If-Match`. Оформлення на основі чернетки не змінює її.

## Структура в коді

```text
src/Sales/
├── Sales.Api/Controllers/AdminOrderWorkspaceController.cs
├── Sales.Application/Contracts/Orders/IAdminOrderWorkspaceService.cs
├── Sales.Domain/Orders/Entities/OrderDraft.cs
└── Sales.Infrastructure/
    ├── Orders/AdminOrderWorkspaceService.cs
    ├── Orders/AdminOrderWorkspaceService.Drafts.cs
    ├── Orders/AdminWorkspacePricing.cs
    └── Migrations/*_AddAdminOrderDrafts.cs
```

## Пов'язані джерела

[Контракт замовлень](../../contracts/sales/admin-order-workspace.openapi.yaml) · [Контракт чернеток](../../contracts/sales/admin-order-drafts.openapi.yaml) · [SDD 004](../../specs/sales/004-admin-order-workspace/README.md) · [SDD 005](../../specs/sales/005-admin-order-drafts/README.md).
