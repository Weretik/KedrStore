# Media CDN sync runbook

`--job=media-cdn-sync` registers pictures that already sit on the CDN in the media library ([specification](../../specs/media/001-media-library/README.md), R-8), then links them to products ([stage 3](../../specs/media/001-media-library/stage-3-product-media.md), R-33, R-34), then lays out product files in folders by category (R-37). It is safe to run again: known files, existing links, and files already in place are skipped. It never deletes or moves CDN files; library folders are virtual.

## Prerequisites

- An FTP user of CDN Express whose home is the CDN root (`MediaStorage:Ftp:Username`, default `cdn3245_media`).
- Its password in GCP secret `cdn-media-ftp-password` with Secret Accessor for the runtime service account; the deploy passes it as `MediaStorage__Ftp__Password` to the API and to the Cloud Run job `media-cdn-sync`.
- Schema `media` migrated (the deploy runs `--job=migrate`).

## Run

1. Dry run: in Cloud Run → Jobs → `media-cdn-sync` → **Execute with overrides**, set the arguments to `--job=media-cdn-sync` and `--dry-run`. The log prints `[RESULT] media-cdn-sync dryRun=True; found=…; known=…; registered=…; notPictures=…; orphans=…; missingOnCdn=…; hiddenSkipped=…` (files in CDN service folders such as `.thumbs/`, never registered) and one `[RESULT] folder …: N new` line per folder, then `[RESULT] product-media-link dryRun=True; photosLinked=…; schemesLinked=…; linksDropped=…; productsChanged=…; withoutPhoto=…`, then `[RESULT] product-media-folders dryRun=True; foldersCreated=…; filesMoved=…; keptOutside=…`. Nothing is saved. The link part sees only files already in the library, so before the first real sync it reports fewer links than the real run makes.
2. Read the numbers: `registered` is what the real run will add; `notPictures` are non-image files; `orphans` are unknown files in `library/` (uploads whose save failed); `missingOnCdn` are library rows whose CDN file is gone. In the link line, `photosLinked`/`schemesLinked` are new links, `linksDropped` are links to files deleted from the library, `productsChanged` counts products whose links or cached photo changed (the first run also clears the old 1C-built URLs of products without a file), and `withoutPhoto` is how many active products stay without a photo. In the folders line, `foldersCreated` are new category folders, `filesMoved` are files that change folder, and `keptOutside` are linked files someone put outside «Товари»/«Схеми» (they stay). The folders part also sees only links that exist now, so before the first real link it reports less.
3. Real run: **Execute** the job with its default arguments (`--job=media-cdn-sync`).
4. Optional daily run: a Cloud Scheduler trigger for the job keeps files uploaded straight over FTP visible in the library.

Locally: `dotnet run --project src/Bootstrapper/Host.Jobs -- --job=media-cdn-sync --dry-run` with `MediaStorage:Ftp:Password` in user secrets and a local database.

## Product links

- Photo: a file directly in `products/` named `<product id>.jpg|jpeg|png|webp` (any case) becomes the first photo of the active product with that ID, if it has no photo yet.
- Scheme: a file directly in `product-scheme/` named `s<product id>.<ext>` becomes its scheme, if it has none.
- Links set on the admin product page are never replaced. Deleted products are not linked. Several files for one ID: the oldest library row wins.
- Every run rereads link URLs from the library, drops links to files that no longer exist, and rewrites `Products.Photo`/`Sсheme` and the list photo.

## Folders by category

- Inside «Товари» (photos) and «Схеми» (schemes) the run builds the category tree with the same names and nesting and moves each linked file into the folder of its product's category. A file shared by several products follows the product with the smallest ID.
- Only files in «Товари»/«Схеми» or their subfolders move, so a file put in another folder by hand stays there. Moving a file by hand inside «Товари» is undone by the next run.
- A renamed category gets a new folder on the next run; the old one stays empty (delete it in the admin if you like). Files of products without a category stay where they are.

## Mapping

`products/` → «Товари», `product-scheme/` → «Схеми», any other top directory keeps its name; deeper directories become subfolders. `avatars/` is never listed. Change the names under `MediaStorage:Sync:FolderNames` only before the first real run; folders created earlier keep their names.
