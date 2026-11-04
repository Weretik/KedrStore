# Media CDN sync runbook

`--job=media-cdn-sync` registers pictures that already sit on the CDN in the media library ([specification](../../specs/media/001-media-library/README.md), R-8). It is safe to run again: known files are skipped. It never deletes or moves CDN files.

## Prerequisites

- An FTP user of CDN Express whose home is the CDN root (`MediaStorage:Ftp:Username`, default `cdn3245_media`).
- Its password in GCP secret `cdn-media-ftp-password` with Secret Accessor for the runtime service account; the deploy passes it as `MediaStorage__Ftp__Password` to the API and to the Cloud Run job `media-cdn-sync`.
- Schema `media` migrated (the deploy runs `--job=migrate`).

## Run

1. Dry run: in Cloud Run → Jobs → `media-cdn-sync` → **Execute with overrides**, set the arguments to `--job=media-cdn-sync` and `--dry-run`. The log prints `[RESULT] media-cdn-sync dryRun=True; found=…; known=…; registered=…; notPictures=…; orphans=…; missingOnCdn=…` and one `[RESULT] folder …: N new` line per folder. Nothing is saved.
2. Read the numbers: `registered` is what the real run will add; `notPictures` are non-image files; `orphans` are unknown files in `library/` (uploads whose save failed); `missingOnCdn` are library rows whose CDN file is gone.
3. Real run: **Execute** the job with its default arguments (`--job=media-cdn-sync`).
4. Optional daily run: a Cloud Scheduler trigger for the job keeps files uploaded straight over FTP visible in the library.

Locally: `dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs -- --job=media-cdn-sync --dry-run` with `MediaStorage:Ftp:Password` in user secrets and a local database.

## Mapping

`products/` → «Товари», `product-scheme/` → «Схеми», any other top directory keeps its name; deeper directories become subfolders. `avatars/` is never listed. Change the names under `MediaStorage:Sync:FolderNames` only before the first real run; folders created earlier keep their names.
