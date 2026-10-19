# Клієнти й контрагенти

**Статична звірка з кодом:** 2026-10-19. **Модуль:** sales.

Admin-список і деталі контрагентів, локальна синхронізація з 1C.

Список `GET /api/admin/customers` застосовує `searchTerm` до ID, імені й телефону та окремі фільтри `counterpartyId`, `name`, `phone` до всієї активної вибірки. Умови поєднуються через AND, `totalRecords` обчислюється до пагінації, а `sort` задає порядок `name`/`counterpartyId`/`phone` у двох напрямках зі стабільним ID. Деталі й синхронізація залишаються окремими сценаріями.

Деталі зберігають ID типів цін і категорій та додають їхні назви з довідників Catalog через пакетний `ICatalogReferenceDataReader`. Відсутній довідниковий запис дає `null` у відповідному полі назви без втрати правила.

## Будова й зв'язки

[AdminCustomersController](../../../../src/Sales/Sales.Api/Controllers/AdminCustomersController.cs) читає через [list handler](../../../../src/Sales/Sales.Application/Features/Customers/GetList/GetCustomerListQueryHandler.cs) і [detail handler](../../../../src/Sales/Sales.Application/Features/Customers/GetById/GetCustomerByIdQueryHandler.cs). [Counterparty](../../../../src/Sales/Sales.Domain/Customers/Entities/Counterparty.cs) є доменною моделлю; [sync service](../../../../src/Sales/Sales.Infrastructure/Integrations/OneC/Services/OneCCounterpartiesSyncService.cs) оновлює її з 1C.

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Sales/
    ├── Sales.Api/Controllers/AdminCustomersController.cs  # HTTP-вхід і query-параметри
    ├── Sales.Application/Features/Customers/
    │   ├── GetById/GetCustomerByIdQueryHandler.cs  # обробник сценарію
    │   └── GetList/GetCustomerListQueryHandler.cs  # обробник сценарію
    ├── Sales.Domain/Customers/Entities/Counterparty.cs  # доменна модель
    └── Sales.Infrastructure/
        ├── Customers/CustomerReadService.cs  # пошук, фільтри, порядок, підрахунок і сторінка
        └── Integrations/OneC/Services/OneCCounterpartiesSyncService.cs  # синхронізація
```

## Схема будови

```mermaid
flowchart LR
    A["AdminCustomersController"] --> B["GetCustomerListQueryHandler"]
    A --> C["GetCustomerByIdQueryHandler"]
    B --> G["CustomerReadService"]
    G --> D["Counterparty"]
    C --> D
    E["1C"] --> F["OneCCounterpartiesSyncService"]
    F --> D
    G --> H["CatalogReferenceDataReader"]
```

## Пов'язані джерела

[Контракт](../../contracts/sales/customer-read.openapi.yaml) · [SDD](../../specs/sales/003-customer-read-api/README.md) · [Runbook](../../operations/jobs/sales-one-c-runbook.md).
