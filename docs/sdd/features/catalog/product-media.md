# Product photos and schemes

**Static code check:** 2026-11-04. **Modules:** Catalog (links), Media (files).

A product has 0–20 photos in order and 0–1 scheme, linked by ID to files of the [media library](../media/media-library.md). The 1C import never writes photo URLs. Specification: [Media 001, stage 3](../../specs/media/001-media-library/stage-3-product-media.md).

## Structure and relationships

The links are owned by the [Product](../../../../src/Modules/Catalog/Catalog.Domain/Products/Product.Media.cs) aggregate as [ProductMediaLink](../../../../src/Modules/Catalog/Catalog.Domain/Products/Media/ProductMediaLink.cs) rows (table `ProductMedia`: product, media file ID, kind, position, URL copy). `Product.Photo` and `Product.Sсheme` stay as a cache of the first photo and the scheme, so lists, sales, and projections remain one-table reads; they are an empty string without media.

Catalog reads files only through its port [IProductMediaLibrary](../../../../src/Modules/Catalog/Catalog.Application/Abstractions/Media/IProductMediaLibrary.cs). Catalog.Infrastructure serves it with the Media contract [IMediaFileDirectory](../../../../src/Modules/Media/Media.Contracts/Files/IMediaFileDirectory.cs). Media calls every [IMediaFileChangeListener](../../../../src/Modules/Media/Media.Contracts/Files/IMediaFileChangeListener.cs) after a replace or delete; the Catalog listener updates the links through [ProductMediaFileChanges](../../../../src/Modules/Catalog/Catalog.Application/Features/ProductMedia/Shared/ProductMediaFileChanges.cs). A failed listener is logged and does not fail the Media request.

- `GET/PUT /api/admin/products/{id}/media` (CanManageProducts + CanManageMedia): read or replace the whole set. Unknown file IDs are `400`.
- `GET /api/admin/products?withoutPhoto=true`: products whose cached photo is empty.
- Public product details return `photo`, `scheme`, and `photos` (gallery order).
- The [link job](../../../../src/Modules/Catalog/Catalog.Application/Features/ProductMedia/Jobs/LinkProductMediaJob.cs) runs inside `--job=media-cdn-sync` after the sync, with the same `--dry-run`. It rereads link URLs, drops links to deleted files, rewrites the cache, and links `products/<id>.<jpg|jpeg|png|webp>` and `product-scheme/s<id>.<ext>` to active products without links of that kind. Then the [folder job](../../../../src/Modules/Catalog/Catalog.Application/Features/ProductMedia/Jobs/ArrangeProductMediaFoldersJob.cs) lays out linked photos in «Товари» and schemes in «Схеми» as the category tree, through `IProductMediaLibrary.ArrangeFoldersAsync` and Media's `IMediaFolderArranger` (R-37). See the [runbook](../../operations/jobs/media-cdn-sync-runbook.md).

Every change copies the cached photo into `ProductListProjections` with one SQL update (`RefreshPhotosAsync`).

## Code structure

```text
src/
├── Modules/Media/Media.Contracts/Files/                 # contract for other modules
│   ├── IMediaFileDirectory.cs                   # files by ID and by CDN directory
│   ├── IMediaFileChangeListener.cs              # replace and delete callbacks
│   └── MediaFileReference.cs
├── Modules/Media/Media.Infrastructure/Files/MediaFileDirectory.cs
├── Modules/Media/Media.Application/Features/Files/Common/MediaFileChangeNotifier.cs
└── Modules/Catalog/
    ├── Catalog.Domain/Entities/{Product.Media,ProductMediaLink}.cs  # links, rules R-30, cache
    ├── Catalog.Application/
    │   ├── Contracts/Media/IProductMediaLibrary.cs             # Catalog port
    │   ├── Features/ProductMedia/{Get,Set,FileChanges,Common}/ # endpoints, listener use case
    │   └── Jobs/ProductMedia/LinkProductMediaJob.cs            # auto-link and cache refresh
    ├── Catalog.Infrastructure/MediaLibrary/                    # port adapter and listener
    └── Catalog.Api/Endpoints/Products/*ProductMediaEndpoint.cs # /{id}/media
```

## Structure diagram

```mermaid
flowchart LR
    A["Product media endpoints /{id}/media"] --> B["Set / Get handlers"]
    B --> P["Product + ProductMediaLink"]
    B --> L["IProductMediaLibrary"]
    L --> D["Media IMediaFileDirectory"]
    M["Media replace / delete"] --> N["IMediaFileChangeListener"]
    N --> C["ProductMediaFileChanges"] --> P
    J["media-cdn-sync"] --> K["LinkProductMediaJob"] --> P
    J --> F["ArrangeProductMediaFoldersJob"] --> L
    P --> R["ProductListProjections.Photo"]
```

## Tests

[ProductMediaTests.cs](../../../../tests/UnitTests/Catalog/Domain/ProductMediaTests.cs) · [SetProductMediaCommandValidatorTests.cs](../../../../tests/UnitTests/Catalog/Application/SetProductMediaCommandValidatorTests.cs) · [ProductMediaApiTests.cs](../../../../tests/IntegrationTests/Catalog/ProductMedia/ProductMediaApiTests.cs) · [ProductMediaLinkPostgresTests.cs](../../../../tests/IntegrationTests/Catalog/ProductMedia/ProductMediaLinkPostgresTests.cs) · [ProductImportSoftDeleteTests.cs](../../../../tests/IntegrationTests/Catalog/Import/ProductImportSoftDeleteTests.cs)

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~ProductMedia"` and `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~ProductMedia"` (the PostgreSQL tests need Docker).

## Related sources

[Contract](../../contracts/catalog/products.openapi.yaml) · [Data](../../architecture/data-model/catalog-products.md) · [Media library](../media/media-library.md).
