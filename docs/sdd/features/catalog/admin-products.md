# Admin-товари й фото

**Статична звірка з кодом:** 2026-10-19. **Модуль:** catalog.

Сторінковий і повний списки товарів, пошук відсутніх/некоректних фото та деталі Admin-товару за slug.

## Будова й зв'язки

[AdminProductsController](../../../../src/Catalog/Catalog.Api/Controllers/AdminProductsController.cs) передає читання [handler списку](../../../../src/Catalog/Catalog.Application/Features/Products/GetAdminList/GetAdminProductListQueryHandler.cs), [handler фото](../../../../src/Catalog/Catalog.Application/Features/Products/GetMissingPhotos/GetMissingProductPhotosQueryHandler.cs) або [handler деталей](../../../../src/Catalog/Catalog.Application/Features/Products/GetAdminDetail/GetAdminProductBySlugQueryHandler.cs). Списки застосовують серверні фільтри та пошук перед підрахунком і пагінацією; boolean-параметри розрізняють відсутнє значення, `true` і `false`. Фото-список поєднує ProductListProjection, Product і [ProductPhotoCheck](../../../../src/Catalog/Catalog.Domain/Entities/ProductPhotoCheck.cs). Деталі поєднують проєкцію, Product і категорію за slug, повертаючи обидві назви, схему та ознаки. HTTP-зміни товарів тут немає.

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Catalog/
    ├── Catalog.Api/Controllers/AdminProductsController.cs  # HTTP-вхід
    ├── Catalog.Application/Features/Products/
    │   ├── GetAdminList/
    │   │   ├── GetAdminProductListQueryHandler.cs  # обробник сценарію
    │   │   └── GetAllAdminProductsQueryHandler.cs  # обробник сценарію
    │   ├── GetMissingPhotos/GetMissingProductPhotosQueryHandler.cs  # обробник сценарію
    │   └── GetAdminDetail/GetAdminProductBySlugQueryHandler.cs  # деталі за slug
    └── Catalog.Domain/Entities/ProductPhotoCheck.cs  # доменна модель
```

## Схема будови

```mermaid
flowchart LR
    A["AdminProductsController"] --> B["GetAdminProductListQueryHandler"]
    A --> C["GetMissingProductPhotosQueryHandler"]
    A --> G["GetAdminProductBySlugQueryHandler"]
    B --> D["ProductListProjection"]
    C --> D
    C --> E["Product"]
    C --> F["ProductPhotoCheck"]
    G --> D
    G --> E
    G --> H["ProductCategory"]
```

## Пов'язані джерела

[Контракт](../../contracts/catalog/products.openapi.yaml) · [SDD списку](../../specs/catalog/admin-product-list/README.md) · [SDD фото](../../specs/catalog/003-missing-product-photos/README.md) · [SDD деталей](../../specs/catalog/004-admin-product-detail/README.md).
