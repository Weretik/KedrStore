# Потік імпорту Catalog

## Повна синхронізація

`SyncOneCFullJob` задає канонічний порядок оновлення. Він читає налаштовані root IDs Doors, Hardware і Cosmos та виконує:

```text
типи цін
  → категорії (doors, hardware, cosmos)
  → деталі товарів (doors, hardware, cosmos)
  → залишки (doors, hardware, cosmos)
  → ціни (doors, hardware, cosmos)
  → одна перебудова ProductListProjections
```

| Job | Читає | Записує | За порожньої відповіді |
| --- | --- | --- | --- |
| Типи цін | Типи цін 1C | `PriceTypes` | Без змін |
| Категорії | Категорії одного root | `Categories` | Зберігає наявні записи; upsert root/manual groups |
| Деталі товарів | Товари одного root + RU CSV | `Products`, `ProductTranslations` | Без змін; якщо непорожня відповідь дає нуль після mapping, перед видаленням кидає виняток |
| Залишки | Залишки одного root | `Products.Stock` | Без змін |
| Ціни | Ціни одного root | `ProductPrices` | Без змін |
| Перебудова проєкції | Локальні дані каталогу | `ProductListProjections` | Замінює модель читання |

Звіряння категорій/товарів може видалити записи, відсутні в непорожній зіставленій відповіді джерела для цього root. Запускай jobs для окремого root лише коли 1C повертає його повністю.

## Правило видимості

`ExportToSite` travels `1C → Products → ProductListProjections`.

- `false` не зупиняє імпорт, оновлення залишків або збереження цін.
- Публічний список товарів фільтрує за `ExportToSite == true`.
- Адміністративні списки не фільтрують за цим прапорцем і повертають `exportToSite`.

`full` вимикає проміжні перебудови й перебудовує проєкцію один раз наприкінці. Цільовий `stocks` job за замовчуванням перебудовує проєкцію після оновлення залишків, тому `InStock` у списку актуалізується. Джерело: `SyncOneCFullJob.cs`, `SyncOneCStocksJob.cs` і `Host.Jobs/Program.cs`, звірено 2026-10-15.
