# Публічні товари

**Статична звірка з кодом:** 2026-10-19. **Модуль:** catalog.

Анонімні списки за мовою/категорією та деталі за slug.

## Будова й зв'язки

[ProductsController](../../../../src/Catalog/Catalog.Api/Controllers/ProductsController.cs) викликає [handler списку](../../../../src/Catalog/Catalog.Application/Features/Products/GetPublicList/GetPublicProductListQueryHandler.cs) і [handler деталей](../../../../src/Catalog/Catalog.Application/Features/Products/GetBySlug/GetProductBySlugQueryHandler.cs). Список спирається на [read-проєкцію](../../../../src/Catalog/Catalog.Domain/Entities/ProductListProjection.cs); джерельний товар — [Product](../../../../src/Catalog/Catalog.Domain/Entities/Product.cs), ієрархія — [ProductCategory](../../../../src/Catalog/Catalog.Domain/Entities/ProductCategory.cs), доступ до БД — [IReadCatalogDbContext](../../../../src/Catalog/Catalog.Application/Contracts/Persistence/ICatalogDbContext.cs).

Коли передано `categoryId` або `categorySlug`, handler читає наявні категорії, вибирає корінь за ID (пріоритетний) чи slug і обходить усіх нащадків за `ParentId` без фіксованої глибини. Публічні проєкції фільтруються за набором ID піддерева до `totalRecords`, сортування та пагінації. Невідома категорія дає порожню сторінку; без параметра категорії зберігається загальний список.

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Catalog/
    ├── Catalog.Api/Controllers/ProductsController.cs  # HTTP-вхід
    ├── Catalog.Application/
    │   ├── Contracts/Persistence/ICatalogDbContext.cs  # порт і контракт
    │   └── Features/Products/
    │       ├── GetBySlug/GetProductBySlugQueryHandler.cs  # обробник сценарію
    │       └── GetPublicList/GetPublicProductListQueryHandler.cs  # обробник сценарію
    └── Catalog.Domain/Entities/
        ├── Product.cs  # доменна модель
        ├── ProductCategory.cs  # ієрархія категорій
        └── ProductListProjection.cs  # модель списку
```

## Схема будови

```mermaid
flowchart LR
    A["ProductsController"] --> B["GetPublicProductListQueryHandler"]
    A --> C["GetProductBySlugQueryHandler"]
    B --> D["IReadCatalogDbContext"]
    C --> D
    D --> E["ProductListProjection"]
    D --> F["Product"]
    D --> G["ProductCategory"]
```

## Пов'язані джерела

[Контракт](../../contracts/catalog/products.openapi.yaml) · [Проєкція](stock-projection.md) · [SDD піддерева категорії](../../specs/catalog/002-category-read-api/requirements/public-product-category-filter.md).
