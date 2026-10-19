# Відсутні фотографії товарів — API та інтеграційний контракт

**Статус:** погоджено

## HTTP-операція

```http
GET /api/admin/products/missing-photos?page=1&pageSize=20
```

- **ID операції:** `getAdminProductsWithUnavailablePhotos`
- **Безпека:** явно анонімний доступ (`AllowAnonymous`, без схеми безпеки)
- **Ідемпотентність:** безпечний GET лише для читання; ключ ідемпотентності не потрібний
- **Джерело даних:** лише PostgreSQL; на шляху запиту немає звернення до CDN
- **Порядок:** `id` товару за зростанням
- **Сторінкова навігація:** `page` за замовчуванням `1`; `pageSize` за замовчуванням `20` та обмежений
  діапазоном `1..100`

Маршрут адитивно додається до наявного `AdminProductsController`. До реалізації
API додайте його в `docs/sdd/contracts/catalog/products.openapi.yaml` і посилання
на шлях операції в `docs/sdd/contracts/openapi.yaml`.

## Відповідь

`200 OK` повертає усталену обгортку `PagedResult`. Кожен рядок містить усі
поля наявного контракту `AdminProduct` і додатково:

| Поле | Тип | Значення |
| --- | --- | --- |
| `photoAvailable` | boolean | Для цього endpoint завжди `false`. |
| `photoStatus` | enum | `Missing`, `InvalidContentType` або `CheckFailed`. |
| `photoCheckedAtUtc` | string/date-time | Час завершення останньої перевірки. |
| `photoHttpStatusCode` | integer або null | Кінцевий HTTP-статус, якщо відповідь було отримано. |

Наявне поле `photo` залишається URL, який перевіряли. Внутрішні діагностичні
повідомлення, текст винятків, заголовки й тіла відповідей не повертаються.

Приклад:

```json
{
  "pagedInfo": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "totalRecords": 1
  },
  "value": [
    {
      "id": 8190,
      "nameUk": "Товар",
      "nameRu": "Товар",
      "productSlug": "tovar-8190",
      "photo": "https://images-kedr.cdn.express/products/8190.jpg",
      "categoryId": 10,
      "inStock": true,
      "isSale": false,
      "isNew": false,
      "exportToSite": false,
      "price": 1000.00,
      "stock": 2.00,
      "quantityInPack": 1,
      "photoAvailable": false,
      "photoStatus": "Missing",
      "photoCheckedAtUtc": "2026-09-23T12:00:00Z",
      "photoHttpStatusCode": 404
    }
  ]
}
```

Порожній результат — це `200` з порожнім `value` і нульовими підсумками.
Некоректна сторінкова навігація повертає усталене тіло помилки валідації `400`.
Операція не використовує `404`, а збої CDN ніколи не стають HTTP-помилкою
цього endpoint читання.

## Зміна 2026-10-19 — параметри серверної таблиці

**Статус:** реалізовано 2026-10-19; цей розділ доповнює початковий контракт за [SC-012–SC-015](../requirements/table-browsing.md).

```http
GET /api/admin/products/missing-photos?searchTerm=8190&inStock=false&isSale=false&isNew=true&exportToSite=true&sort=NameAsc&page=1&pageSize=20
```

Усі нові параметри необов'язкові. `searchTerm` має не більш як `100` символів; boolean-параметри `inStock`, `isSale`, `isNew`, `exportToSite` розрізняють відсутнє значення, `true` і `false`. `sort` приймає `IdAsc`, `IdDesc`, `NameAsc`, `NameDesc`, `PriceAsc`, `PriceDesc`, за замовчуванням `IdAsc`. Сортування має завершальний ключ ID. Фільтрація й підрахунок передують пагінації; форма `PagedResult`, поля рядка, доступ та помилка `400` залишаються тими самими. Адитивну зміну внесено до машинозчитуваного [OpenAPI](../../../../contracts/catalog/products.openapi.yaml). Міграція, CDN-запити й зміна job не потрібні.

## CLI-контракт Host.Jobs

```text
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=check-product-photos
```

Команда не приймає `rootId`, виконує один повний цикл для активних товарів,
записує підсумок у журнал і завершується кодом `0` після оркестрації та
збереження. Віддалені збої окремих товарів зберігаються як `CheckFailed`
і не призводять до помилки процесу. Некоректна конфігурація, збій бази даних,
скасування чи необроблена помилка повертають ненульовий код помилки хоста.

## Інтеграційний контракт Cloud Run

Workflow створює або оновлює `catalog-product-photo-check` із поточного образу
Host.Jobs з наведеним вище аргументом CLI, однією задачею, паралелізмом один,
нульовою кількістю повторів платформи, таймаутом `30m` і лише секретом бази даних
Catalog. Розгортання не виконує й не планує запуск цієї задачі.

## Сумісність і розгортання

HTTP-маршрут, таблиця, команда й Cloud Run Job додаються без порушення наявного.
Поточні маршрути товарів і DTO не змінюються. Споживачі можуть перейти на endpoint
після першого завершеного запуску задачі; до того рядки `Unknown` навмисно відсутні.
Авторизацію явно відкладено; вона потребуватиме майбутньої зміни контракту.
