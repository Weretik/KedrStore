# Catalog categories

**Static code check:** 2026-10-30. **Module:** Catalog.

Public tree and category page by slug, Admin reads, Admin slug edit, and Admin-written SEO text and FAQ per language.

## Structure and relationships

The [public controller](../../../../src/Catalog/Catalog.Api/Controllers/CategoriesController.cs) and [admin controller](../../../../src/Catalog/Catalog.Api/Controllers/AdminCategoriesController.cs) dispatch to Application handlers. [ProductCategory](../../../../src/Catalog/Catalog.Domain/Entities/ProductCategory.cs) is soft-deleted by the 1C import (a global query filter hides it) and keeps its slug for life; former slugs live in [SlugHistoryEntry](../../../../src/Catalog/Catalog.Domain/Entities/SlugHistoryEntry.cs) and resolve through [SlugService](../../../../src/Catalog/Catalog.Application/Slugs/SlugService.cs). Product counts and the price range come from the product list projection through [CategoryProductStats](../../../../src/Catalog/Catalog.Application/Features/Category/CategoryProductStats.cs). [CategoryContent](../../../../src/Catalog/Catalog.Domain/Entities/CategoryContent.cs) holds SEO title, meta description, H1, sanitized body, and FAQ per language; the body passes the [HTML sanitizer](../../../../src/Catalog/Catalog.Infrastructure/Content/HtmlSanitizerService.cs) on save.

Access: reads need `CanManageProducts`; slug edit, content save, and `deleted=true` need the Admin role.

## Code structure

The tree lists verified entry points and key feature components; it does not include every supporting file.

```text
src/
└── Catalog/
    ├── Catalog.Api/Controllers/
    │   ├── AdminCategoriesController.cs  # Admin HTTP: list, by id, slug, content
    │   ├── CategoriesController.cs  # public HTTP: tree, by slug
    │   └── CatalogLookupActionResult.cs  # 404 with redirect body
    ├── Catalog.Application/
    │   ├── Features/Category/
    │   │   ├── AdminGetById/, AdminGetList/  # Admin reads (+ deleted filter)
    │   │   ├── Content/Get/, Content/Save/  # Admin content query and command
    │   │   ├── GetBySlug/GetCategoryBySlugQueryHandler.cs  # history, redirect, counts, prices, content
    │   │   ├── GetList/GetCategoriesQuryHandler.cs  # tree with counts, hides empty
    │   │   ├── CategoryProductStats.cs  # counts and price range, rolled up the tree
    │   │   └── CategoryTreeIds.cs  # category + descendants
    │   ├── Features/Slugs/Change/  # Admin slug edit (products and categories)
    │   └── Slugs/  # transliteration, slug text, allocator, slug service
    ├── Catalog.Domain/Entities/
    │   ├── ProductCategory.cs  # soft delete, slug change
    │   ├── CategoryContent.cs, CategoryFaqItem.cs  # content and limits
    │   └── SlugHistoryEntry.cs  # former slugs
    └── Catalog.Infrastructure/
        ├── Configurations/  # CategoryContent, SlugHistoryEntry, ProductCategory mappings
        └── Content/HtmlSanitizerService.cs  # allow-list sanitizer
```

## Structure diagram

```mermaid
flowchart LR
    A["CategoriesController"] --> C["GetCategoriesQuryHandler / GetCategoryBySlugQueryHandler"]
    B["AdminCategoriesController"] --> D["Admin reads, ChangeSlug, Content"]
    C --> S["SlugService"]
    C --> T["CategoryStatsReader"]
    D --> S
    D --> H["IHtmlSanitizerService"]
    C & D & S & T --> F["CatalogDbContext"]
```

## Tests

[CategoryPresentationApplicationTests.cs](../../../../tests/UnitTests/Catalog/Application/CategoryPresentationApplicationTests.cs) · [SlugTextTests.cs](../../../../tests/UnitTests/Catalog/Slugs/SlugTextTests.cs) · [ImportSyncTests.cs](../../../../tests/UnitTests/Catalog/Sync/ImportSyncTests.cs) · [CatalogSoftDeleteAndContentTests.cs](../../../../tests/UnitTests/Catalog/Domain/CatalogSoftDeleteAndContentTests.cs) · [CatalogStableUrlsApiTests.cs](../../../../tests/IntegrationTests/Catalog/Api/CatalogStableUrlsApiTests.cs) · [CategoryPresentationPersistenceTests.cs](../../../../tests/IntegrationTests/Catalog/Persistence/CategoryPresentationPersistenceTests.cs)

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Catalog"` and `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~CatalogStableUrlsApiTests"` (in-memory, no Docker).

## Related sources

[Contract](../../contracts/catalog/category-read.openapi.yaml) · [Stable URLs and category content SDD](../../specs/catalog/005-stable-urls-and-category-content/README.md) · [Category read SDD](../../specs/catalog/002-category-read-api/README.md) · [Category metadata SDD](../../specs/catalog/001-category-presentation-metadata/README.md).
