# API-контракти

`docs/sdd/contracts/` — канонічне версійне джерело машинозчитуваних публічних API-контрактів.

## Структура

```text
docs/sdd/contracts/
├── openapi.yaml                         # сукупна точка входу для публічних API
└── <module>/
    └── <feature>.openapi.yaml           # контракт однієї фічі
```

`openapi.yaml` — точка входу для Swagger UI, генерації frontend-клієнта й контрактних тестів. Він підключає модульні контракти через `$ref`; не дублюй операції чи схеми в сукупному файлі.

## Поточна передача контракту до frontend

- [Посібник інтеграції frontend](frontend-integration.md) описує автентифікацію, CSRF, pagination, помилки та власників endpoints.
- [Сукупний контракт OpenAPI 3.1](openapi.yaml) охоплює поточні зареєстровані controllers.
- Catalog: [товари](catalog/products.openapi.yaml), [категорії](catalog/category-read.openapi.yaml), [швидке замовлення](catalog/orders.openapi.yaml).
- Sales: [каталог для менеджера](sales/catalog.openapi.yaml), [замовлення й синхронізація з 1C](sales/admin-order-one-c-sync.openapi.yaml), [читання замовлень](sales/manager-order-read.openapi.yaml), [читання клієнтів](sales/customer-read.openapi.yaml).
- Identity: [сеанс](identity/session.openapi.yaml).

OpenAPI-файли описують контракт поточного коду. Вони самі не відкривають доступ до кінцевих точок: авторизаційні атрибути й резервне правило доступу хоста залишаються визначальними.

У `contracts/api-contract.md` кожної SDD-фічі фіксуй рішення про обсяг, споживачів, сумісність, безпеку, помилки, ідемпотентність і розгортання. Для нової чи зміненої HTTP-поведінки узгодь ці рішення й, коли практично можливо, створи або онови машинозчитуваний контракт до реалізації транспортного рівня. За узгодженим контрактом додай цільові Red-тести API/контракту, тоді реалізуй транспортний рівень. Документ фічі посилається на `docs/sdd/contracts/<module>/<feature>.openapi.yaml`; сукупний `openapi.yaml` оновлюється в тій самій зміні.

Не створюй публічний OpenAPI-документ для суто внутрішньої зміни. До додавання HTTP-контракту узгодь route, method, operation ID, DTO, приклади, помилки, безпеку, ідемпотентність записів із побічними ефектами та шлях міграції для breaking changes.
