# Модель даних і backend design

## Базис

Перед реалізацією у Sales snapshot були `Orders`, `OrderLines`, `OneCOrderSyncs`; таблиць чернеток не було. Чернеткові таблиці додає ця фіча. Чернетки — нова модель Sales, не статус чинного Order.

## Плановані дані

```text
SalesDbContext (поточна PostgreSQL схема)
├── OrderDrafts
│   ├── Id bigint PK (Domain: typed DraftId)
│   ├── CounterpartyId string FK -> Counterparties.Id
│   ├── Title varchar(200) NOT NULL
│   ├── Note text NULL
│   ├── CreatedAt, UpdatedAt timestamptz
│   └── Version concurrency token
├── OrderDraftLines
│   ├── Id bigint PK (Domain: typed DraftLineId)
│   ├── DraftId bigint FK -> OrderDrafts.Id ON DELETE CASCADE
│   ├── ProductId string
│   └── Quantity integer
└── OrderDraftIdempotencyRecords
    ├── Id bigint PK
    ├── DraftId bigint FK -> OrderDrafts.Id ON DELETE CASCADE
    └── IdempotencyKey, RequestHash, CreatedAtUtc
```

Ідентифікатори — за [стратегією](../../../../standards/identifier-strategy.md); ProductId — посилання на Catalog без DB FK, як в OrderLines. Кількість відповідає чинному `OrderLine.Quantity` типу `int` і має бути додатною. Унікальність `(DraftId, ProductId)`; індекси `(CounterpartyId, UpdatedAt DESC, Id DESC)`, `(UpdatedAt DESC, Id DESC)`. Індекс для title/name search додавати лише за `EXPLAIN` з репрезентативними даними; при текстовому пошуку по Counterparty перевірити join-план. Ціна й сума не зберігаються в чернетці як авторитетні дані; їх обчислює pricing service [004](../../004-admin-order-workspace/design/backend.md).

## Міграція та сумісність

Нова адитивна EF migration створює три таблиці; старі Orders і 1C job не змінюються. Rollout: міграція перед Admin deployment; rollback UI/API без видалення таблиць або даних, відкат БД лише окремим безпечним рішенням. Оновити SalesDbContext, snapshot, карту БД і Sales feature після реалізації. Конкурентне редагування — token у відповідях, `If-Match` або version у PUT/DELETE, передавати поточну `version` у заголовку `If-Match` за плановим OpenAPI. Для видалення застосувати авторизацію та транзакцію.
