# API читання клієнтів — модель даних

## Контекст

Фіча читає наявну локальну проєкцію клієнтів Sales, яку створює чинна синхронізація. Вона не додає збережених даних і не змінює зв'язок з Identity або 1C.

## Модель

```text
Counterparty (existing Sales aggregate; Id: string)
├── IdentityUserId (Guid; Identity-owned referenced user)
├── Name
├── Email
├── Phone (nullable)
├── DefaultPriceTypeId
├── IsDeleted / audit timestamps
└── 0..* CounterpartyCategoryPriceType
    ├── CounterpartyId
    ├── CategoryId
    └── PriceTypeId
```

## Інваріанти та цілісність

- `Counterparty.Id` — наявний стабільний ідентифікатор контрагента Sales/1C і ключ пошуку деталей.
- `Counterparty` підтримує логічне видалення; звичайне читання виключає записи з `IsDeleted`.
- Ключ `CounterpartyCategoryPriceType` складається з `CounterpartyId` і `CategoryId`; цей запис представляє правило для окремої категорії, яке може перевизначати типовий тип ціни контрагента.
- `IdentityUserId` залишається посиланням на Identity. Це не ідентифікатор для споживачів API клієнтів, і його не можна відкривати, якщо цього не вимагає погоджений контракт API.
- Міграцію чи нову структуру збереження даних не заплановано. Під час перевірки реалізації потрібно дослідити посторінковий вивід активних клієнтів з упорядкуванням за `Name` і `Id`; індекс слід додавати лише тоді, коли показові вимірювання в PostgreSQL підтвердять його необхідність.
