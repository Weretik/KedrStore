# Media 001, stage 3 — Products linked to media files

**Status:** merged 2026-11-04 (KedrStore #157, kedr-web #42); folders by category (R-37) added after it, waits for review. Feature card: [Product photos and schemes](../../../features/catalog/product-media.md). **Modules:** Catalog (owner of the links), Media (files, contract). Parent: [Media 001](README.md).

## Goal

The owner asked (2026-11-04) that a product has several photos (a site gallery; one is fine) and one scheme, linked by ID to files of the media library instead of URLs built by the 1C import; that the files already on the CDN are linked to the current products; that photos are attached on the admin product page (upload or pick from the library); and that the photo check job and its table go away, because a product without a linked photo is now known directly.

## Decisions

- Catalog owns the links (`ProductMedia`: product, media file ID, kind, position). Media knows nothing about products. Catalog reads files only through the new `Media.Contracts` (`IMediaFileDirectory`), never Media tables.
- The product keeps its `Photo` and `Sсheme` columns as a **cache** of the first photo URL and the scheme URL, so list, sales, and projection reads stay one-table reads. The links are the source; the cache is rewritten whenever links change.
- Existing HTTP fields stay strings: `photo` and `scheme` are an **empty string** when the product has none (old mobile builds and the storefront parse them as required strings). New field `photos` (ordered URLs) on product details.
- After its own save, Media calls every `IMediaFileChangeListener` (from `Media.Contracts`) on replace and delete; Catalog's listener updates the links and the cache. A plain interface, not a Mediator notification: the architecture rules keep other modules' Domain/Application away from Media, Host.Jobs has no Mediator, and a failing listener must not fail the Media request (it is logged). The next CDN sync run repairs the cache (R-34).
- The CDN sync run (`--job=media-cdn-sync`) links files after it registers them, in the same run and with the same `--dry-run`. One Cloud Run job, one schedule.
- Linking rights follow the library: `CanManageMedia` (Manager, Admin).
- The missing-photo endpoint, the photo check job, its table, and its Cloud Run job are removed. The admin product list gets the filter `withoutPhoto=true` instead. Installed mobile builds lose their «Без фото» screen until the app is updated (accepted by the owner's decision to remove the check).

## Rules

- R-30: a product has 0–20 photos in a stated order and 0–1 scheme. One media file appears at most once per product; the same file may serve several products.
- R-31: `PUT /api/admin/products/{id}/media` replaces the whole set (`photoIds` in order, `schemeId` or `null`); every ID must exist in the library (`400` otherwise). The answer and `GET` return the set with URLs and file names.
- R-32: the 1C import no longer writes photo or scheme URLs; a new product starts without media. The import never changes links.
- R-33: auto-link, for each product that has no photo link (or no scheme link): a library file with key `products/<id>.<jpg|jpeg|png|webp>` (case-insensitive, root of the folder) becomes its first photo; `product-scheme/s<id>.<ext>` becomes its scheme. A product that already has links of that kind keeps them; files of deleted products are left alone.
- R-34: every sync run also refreshes the cache: link URLs are reread from the library, links to files that no longer exist are dropped, and `Photo`/`Sсheme` are rewritten from the links (empty without them).
- R-35: replacing a library file updates the URL in every product that uses it; deleting a library file removes its links.
- R-36: `withoutPhoto=true` on the admin product list returns products whose cached photo is empty.
- R-37 (owner, 2026-11-04): every sync run lays out linked files as folders that repeat the category tree (same names and nesting): photos under «Товари», schemes under «Схеми». A file goes to the folder of the category of its active product with the smallest ID. Only files inside «Товари»/«Схеми» or their subfolders move; a file someone put in another folder stays. Folders are made only for categories that get files and their parents; empty folders are never deleted. URLs never change (folders are virtual).

## Scenarios

- SC-30: the sync dry run reports how many photos and schemes it would link and how many products stay without a photo; it saves nothing.
- SC-31: after a real run, product 731 with `products/731.jpg` and `product-scheme/s731.jpg` in the library has one photo and the scheme; the public product shows both URLs; product 732 without files has `photo = ""`; a second run links nothing new.
- SC-32: a Manager sets photos `[B, A]` and scheme `C` for a product; `GET` returns them in that order; the list photo is B's URL; an unknown file ID is `400`; a customer gets `403`.
- SC-33: replacing file A changes the product's photo URL; deleting it removes the link and moves the next photo to first place.
- SC-34: the 1C import of a known product keeps its links and cache; a new product has no photo.
- SC-35: `withoutPhoto=true` lists only products without a photo.
- SC-36: photos of products in «Фурнітура / Завіси» and «Фурнітура / Ручки» move into «Товари / Фурнітура / Завіси» and «… / Ручки»; a file in «banners» stays; a dry run counts and saves nothing; a second run moves nothing; a file shared by two products follows the smaller product ID.

## Design

- `Media.Contracts` (new project): `IMediaFileDirectory` (`GetByIdsAsync`, `ListInStorageDirectoryAsync`) returning `MediaFileReference(Id, StorageKey, FileName, Url)`, implemented by `Media.Infrastructure`; `IMediaFileChangeListener` (`OnFileReplacedAsync`, `OnFileDeletedAsync`), called by `MediaFileChangeNotifier` in the replace and delete handlers.
- `Catalog.Application/Contracts/Media/IProductMediaLibrary` is Catalog's own port; `Catalog.Infrastructure/MediaLibrary` serves it through the Media contract and implements the listener.
- `Catalog.Domain`: `ProductMediaLink` (owned by `Product`, table `ProductMedia`); `Product.SetMedia`, `LinkPhotoIfNone`, `LinkSchemeIfNone`, `RefreshMedia`, `UpdateMediaUrl`, `RemoveMediaFile`; photo/scheme no longer required.
- `Catalog.Application`: `Features/ProductMedia/{Get,Set}` with validators; `Features/ProductMedia/FileChanges/ProductMediaFileChanges`; `Jobs/ProductMedia/LinkProductMediaJob` (R-33, R-34). The list photo follows with one SQL update (`IProductListProjectionRebuilder.RefreshPhotosAsync`).
- Host.Jobs `media-cdn-sync`: sync, then link, then folders (R-37); one `[RESULT]` line for each part.
- R-37: `Media.Contracts/Folders/IMediaFolderArranger` (implemented by `Media.Infrastructure/Folders/MediaFolderArranger`), reached by Catalog through `IProductMediaLibrary.ArrangeFoldersAsync`; `Catalog.Application/Jobs/ProductMedia/ArrangeProductMediaFoldersJob` builds the tree and placements.
- Migration `ProductMediaLinks`: table `ProductMedia` (public schema, Catalog history), drop `ProductPhotoChecks`.
- Removed: `ProductPhotoCheck*`, `ProductPhotoStatus`, `IProductPhotoProbe`, `CheckProductPhotosJob`, `GetMissingProductPhotos*`, `GET /api/admin/products/missing-photos`, Host.Jobs `check-product-photos`, Cloud Run job `catalog-product-photo-check` (the deploy no longer creates it; the owner deletes it and its schedule).

## Rollout and rollback

Order: kedr-web change that treats `photo = ""` as no photo and uses `withoutPhoto` goes with this change; deploy KedrStore (migration adds `ProductMedia`, drops `ProductPhotoChecks`), then run `media-cdn-sync --dry-run`, read the link report, and run it without the flag after the owner's «да». Until that run every product keeps its old URL. Rollback: revert; the old URLs are still in `Photo`/`Sсheme` only if the real run has not happened, otherwise the next 1C import will not restore them (the old mapper rebuilt them), so a rollback after the run needs the old import once.

## Tasks and evidence

| Task | Rules | Evidence |
| --- | --- | --- |
| TS-30 Media contract and listener | R-35 | `ProductMediaApiTests` (replace and delete over HTTP), `MediaPostgresTests` (directory) |
| TS-31 Catalog links, endpoints, validators | R-30, R-31, R-36 | `ProductMediaTests`, `SetProductMediaCommandValidatorTests`, `ProductMediaApiTests` |
| TS-32 Auto-link and cache refresh in the sync run | R-33, R-34 | `ProductMediaLinkPostgresTests` |
| TS-33 1C import without URLs | R-32 | `ProductImportSoftDeleteTests` (SC-34) |
| TS-35 Folders by category | R-37 | `MediaPostgresTests` (arranger), `ProductMediaLinkPostgresTests` (placements) |
| TS-34 Remove photo check | — | architecture tests, `ApiContractTests`, full integration suite 229/229 on 2026-11-04 |
