# CLI Host.Jobs

`Host.Jobs` завантажує `appsettings.json` і змінні середовища. У Development він використовує **власні** User Secrets, окремі від `Host.Api`. Із кореня репозиторію:

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=<name>
```

| Job | Аргумент | Призначення |
| --- | --- | --- |
| `migrate` | — | послідовні EF-міграції зареєстрованих контекстів, потім сидери; змінює схему й дані |
| `full` | — | Повне оновлення Catalog для налаштованих root IDs Doors, Hardware і Cosmos |
| `pricetypes` | — | Типи цін Catalog |
| `category`, `productdetails`, `stocks`, `prices` | Один або кілька `--rootId=<id>` | Цільове оновлення root Catalog |
| `rebuild-projections` | — | Перебудова моделі читання списку Catalog |
| `check-product-photos` | — | Перевірка URL фото кожного активного товару й збереження останнього статусу |
| `counterparties` | — | Контрагенти Sales |
| `counterparty-category-price-types` | — | Цінові правила клієнтів Sales |
| `sales-customers-full` | — | Спершу контрагенти, потім цінові правила |
| `sync-one-c-orders` | — | Один готовий до виконання batch збережених замовлень менеджерів Sales |

Приклад:

```powershell
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=stocks --rootId=<one-c-root-id>
```

Повтори `--rootId` для кількох коренів. Код виходу `0` і `[SUCCESS] Job finished OK` означають відсутність винятку; окремо перевір кількість імпортованих записів. Код `1` означає помилку job, аргументу або виконання; код `2` — відсутній `--rootId` для job, що його потребує. Для `migrate` звір результат за [інструкцією з міграцій](../data/migrations-and-seeding.md).

Передумови доставки замовлень, тлумачення результату, зупинку й обробку dead-letter див. в [операційній інструкції Sales/1C](sales-one-c-runbook.md#доставка-замовлень-менеджера).

Класифікацію CDN, результат, конфігурацію й запуск Cloud Run описує [операційна інструкція з перевірки фото Catalog](catalog-photo-check-runbook.md).

## Ручний smoke test запису в 1C

Ця команда виконує реальний запис `CreateSiteRequest`. Використовуй її лише з тестовою кінцевою точкою 1C і тестовими даними контрагента/товару. `OneCSoap:Endpoint`, `Username` і `Password` надавай через User Secrets `Host.Jobs` або змінні середовища; не записуй їх в `appsettings.json` чи аргументи команди.

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=one-c-smoke-write --counterpartyId=000000298 --productId=8190 --quantity=10 --amount=1000 --orderNumber=TEST-SO-20260818-001 --comment='TEST: manual SOAP verification.'
```

Числові ID товарів доповнюються початковими нулями до дев'яти символів: `8190` передається як `000008190`. Підтверджений ID документа 1C дає код виходу `0`; бізнес-відхилення — код `3`.

Записані свідчення live-тесту, тлумачення відповіді й карту коду зберігає [smoke-перевірка запису 1C](../../specs/sales/001-admin-order-one-c-sync/tasks/test/06.3-one-c-write-smoke-test.md) у специфікації фічі.
