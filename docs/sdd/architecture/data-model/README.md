# Карта бази даних KedrStore

**Перевірено:** 2026-10-21 за трьома EF Core `DbContext`, конфігураціями й snapshots у `src/`. Це структура моделі в репозиторії, а не підтвердження застосованих міграцій у конкретному середовищі.

Усі три контексти використовують таблиці **без явно заданої PostgreSQL-схеми**. Отже, у моделі немає окремих схем для Catalog, Sales та Identity: назви таблиць належать одній схемі за замовчуванням для підключення. За стандартного `search_path` це `public`; налаштування конкретної розгорнутої БД тут не перевірялося.

```text
KedrStore PostgreSQL
└── схема за замовчуванням (зазвичай public)
    ├── Catalog / CatalogDbContext — 7 таблиць
    │   ├── Категорії
    │   │   └── ProductCategories          # дерево категорій, slug і назви
    │   ├── Товари й ціни
    │   │   ├── Products                   # товар і CategoryId
    │   │   ├── ProductPrices              # ціни товару
    │   │   ├── ProductTranslations        # локалізовані назви
    │   │   └── PriceType                  # типи цін
    │   ├── Проєкція списку
    │   │   └── ProductListProjections     # денормалізоване читання товарів
    │   └── Перевірка фото
    │       └── ProductPhotoChecks         # результат перевірки фото
    ├── Sales / SalesDbContext — 10 таблиць
    │   ├── Клієнти
    │   │   ├── Counterparties             # контрагенти
    │   │   └── CounterpartyCategoryPriceTypes # тип ціни за категорією
    │   └── Замовлення й 1C
    │       ├── Orders                     # замовлення
    │       ├── OrderLines                 # позиції замовлення
    │       ├── OneCOrderSyncs             # стан синхронізації з 1C
    │       ├── OrderIdempotencyRecords   # захист від повторного запиту
    │       ├── OrderSyncRetryAudits      # аудит повторних спроб
    │       ├── OrderDrafts               # збережені чернетки без номера замовлення
    │       ├── OrderDraftLines           # позиції чернеток
    │       └── OrderDraftIdempotencyRecords # захист створення чернетки від повтору
    ├── Identity / AppIdentityDbContext — 8 таблиць
    │   └── Доступ і сеанси
    │       ├── AspNetUsers               # користувачі
    │       ├── AspNetRoles               # ролі
    │       ├── AspNetUserRoles           # призначення ролей
    │       ├── AspNetUserClaims          # claims користувачів
    │       ├── AspNetRoleClaims          # claims ролей
    │       ├── AspNetUserLogins          # зовнішні входи
    │       ├── AspNetUserTokens          # токени
    │       └── AppRefreshSessions       # refresh-сесії
    └── службова інфраструктура
        └── __EFMigrationsHistory         # історія застосованих EF-міграцій
```

## Відкрити лише потрібну частину

| Питання | Документ | Пов'язана фіча |
| --- | --- | --- |
| Дерево категорій | [Категорії](catalog-categories.md) | [Категорії каталогу](../../features/catalog/categories.md) |
| Товари, переклади й ціни | [Товари й ціни](catalog-products.md) | [Публічні товари](../../features/catalog/public-products.md), [імпорт 1C](../../features/catalog/one-c-import.md) |
| Дані списку та залишку | [Проєкція списку](catalog-stock-projection.md) | [Проєкція товарів](../../features/catalog/stock-projection.md) |
| Статус фото | [Перевірка фото](catalog-photo-check.md) | [Перевірка фото](../../features/catalog/photo-check.md) |
| Контрагенти та їхні ціни | [Клієнти](sales-customers.md) | [Клієнти й контрагенти](../../features/sales/customers.md) |
| Замовлення, чернетки й синхронізація | [Замовлення](sales-orders.md) | [Замовлення менеджера](../../features/sales/admin-orders.md), [робоча область Admin](../../features/sales/admin-order-workspace.md) |
| Користувачі та сеанси | [Доступ і сеанси](identity-access.md) | [Сеанс користувача](../../features/identity/session.md) |

Кожну таблицю EF-моделі докладно описано в **одному** документі вище. Фізичні FK показані в його ER-діаграмі; використання ID без FK пояснено окремо. `__EFMigrationsHistory` — службова таблиця, яка не входить до 25 таблиць EF-моделей; її наявність у конкретному середовищі залежить від застосування міграцій. Після зміни схеми оновлюй лише відповідний документ і це дерево, якщо змінився перелік таблиць або розміщення.
