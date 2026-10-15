# Контракт клієнта 1C і нормалізація

`OneCSoapClientFactory` створює згенерований SOAP/WCF-клієнт; `BasicAuthEndpointBehavior` додає Basic authentication. Endpoint, Authorization header, username і пароль не записують у логи чи документацію.

`Catalog.Infrastructure.Integrations.OneC.Client.OneCClient` реалізує абстракцію Application `IOneCClient`. Кожен метод створює SOAP-клієнт, викликає одну згенеровану операцію й відображає значення в DTO модуля.

| Метод Application | Згенерована SOAP-операція | Споживач |
| --- | --- | --- |
| `GetPriceTypesAsync()` | `GetPriceTypesAsync` | `SyncOneCPriceTypesJob` |
| `GetCategoriesAsync(rootId)` | `GetCategoriesAsync` | `SyncOneCCategoryJob` |
| `GetProductDetailsAsync(rootId)` | `GetProductDetailsAsync` | `SyncOneCProductDetailsJob` |
| `GetProductStocksAsync(rootId)` | `GetProductStocksAsync` | `SyncOneCStocksJob` |
| `GetProductPricesAsync(rootId)` | `GetProductPricesAsync` | `SyncOneCPricesJob` |

`rootId` зберігає вихідний рядковий ID 1C, включно з початковими нулями.

## Нормалізація на межі Infrastructure

| Значення | Поточна обробка | Порожнє/некоректне |
| --- | --- | --- |
| ID | Прибрати початкові нулі; розібрати ціле число | `0`, необов'язковий parent ID — `null` |
| Текст | Обрізати пробіли | Порожній рядок |
| Boolean | `true`, `1`, `yes`, українське/російське ствердне значення | `false` |
| Ціна | Розібрати decimal | `0m` |
| Залишок | Decimal за українською, російською, потім invariant cultural convention; округлення вгору | `0m` |

Mapper зберігає `ExportToSite=false`: товар приховано з публічного каталогу, але він залишається в локальному сховищі.

Коли додаєш поле, підтвердь його семантику в джерелі, відобрази в DTO модуля (не WCF-тип), визнач поведінку для некоректного значення, онови [безпеку імпорту](data-safety.md) й додай mapping/інтеграційний тест, де це підтримується.
