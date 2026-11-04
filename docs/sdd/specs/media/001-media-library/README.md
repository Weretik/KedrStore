# Media 001 — Media library for CDN photos

**Status:** stage 1 (backend library) implemented 2026-11-04, waits for review (branch `claude/project-thread-09y8qq`). **Module:** Media (new).

## Goal and boundaries

The owner asked (2026-11-04) to manage the photos that already sit on the CDN (`images-kedr.cdn.express`, folders `products/` and `product-scheme/`) from the admin: one shared library with folders, full management (folders and files), product photos and schemes linked to a product by ID instead of a URL built by the 1C import, several photos per product (a site gallery), and no separate photo check job.

The work has four stages, each with its own PRs and structure check:

1. **Backend library (this spec, R-1…R-9).** Module Media: folders, files, upload, replace, move, rename, delete, paged list, CDN sync job that registers the existing files.
2. **Admin «Медіа».** Folder tree, grid, search, side panel, drag-and-drop upload (kedr-web).
3. **Products + media.** Catalog links products to media files by ID (photos with order, one scheme), auto-links the imported files by name (`731.jpg` → photo of product 731, `s731.jpg` → its scheme), the 1C import stops writing photo URLs, product page picks or uploads photos, the photo check job and its table are removed. A revision of this spec will hold its rules.
4. **Previews.** Small images for grids and tables.

Out of scope: avatars (Identity owns `avatars/`), video and documents, image editing.

## Module decision

A new module `Media` (Domain, Application, Infrastructure, Api) under the [module rules](../../../standards/module-rules.md): files and folders have rules of their own (CDN storage, names, folders) and are used by Catalog now and by Content later. Alternatives: Catalog (rejected: catalog is 1C-driven product data and the library also serves non-product images) and Content (rejected: site texts, not files). Tables live in schema `media` with their own migration history; no other module is referenced. Catalog will refer to files by `MediaFileId` only (stage 3), through a Media contract.

## Rules

- R-1: a folder has a name (1–100 characters, trimmed) unique among its siblings (case-insensitive) and an optional parent; the library root is not a row. A folder cannot be moved into itself or its descendant.
- R-2: folders are virtual: moving a file or folder in the library never moves the file on the CDN, so public URLs stay stable.
- R-3: a file has a CDN key (unique), a display name (1–200), a content type, a size, optional width and height, and an optional folder (`null` = library root).
- R-4: an upload is JPEG, PNG, or WebP (checked by content, not by name) up to 15 MB. It is stored unchanged at `library/<yyyy>/<MM>/<guid>.<ext>`; the display name is the original file name.
- R-5: replace uploads the new picture under a new key, keeps the file ID (so links stay), and deletes the old CDN file. A new key means no CDN or browser cache shows the old picture.
- R-6: delete removes the CDN file and the row. A folder can be deleted only when it has no files and no subfolders.
- R-7: the list is paged on the server (page size 1–200, default 60), filtered by folder (or all), and searched by display name (case-insensitive contains).
- R-8: the CDN sync job lists the CDN recursively, skips `avatars/` and `library/`, and registers every unknown JPEG/PNG/WebP/GIF file: the first directory level becomes a top folder (`products` → «Товари», `product-scheme` → «Схеми», other names as they are), deeper levels become subfolders. Known keys are skipped; files missing on the CDN are only reported. `--dry-run` reports and saves nothing.
- R-9: reads, upload, replace, move, rename, and folder create/rename/move need `CanManageMedia` (Manager, Admin); file and folder delete need `RequireAdminRole` (Admin).

## Scenarios

- SC-1: after `--job=migrate` the database has schema `media` with empty `MediaFolders` and `MediaFiles`.
- SC-2: `--job=media-cdn-sync --dry-run` prints how many files it found per folder, how many are new and how many known rows are missing on the CDN, and saves nothing; without `--dry-run` it creates folders «Товари» and «Схеми» with the files; a second run adds nothing.
- SC-3: a Manager creates folder «Акції» inside «Товари», renames it, and moves it to the root; moving «Товари» into «Акції» is `400`.
- SC-4: a Manager uploads a JPEG into a folder and sees it in the list of that folder with its URL, size, and dimensions; a text file named `.jpg` is `400`.
- SC-5: a Manager replaces a file: the ID stays, the URL changes, the old CDN file is deleted.
- SC-6: a Manager moves several files to another folder and renames one.
- SC-7: a Manager gets `403` on file and folder delete; an Admin deletes a file (CDN file removed) and an empty folder; a folder with files is `409`.
- SC-8: an anonymous user gets `401` and a customer `403` on `/api/admin/media/*`; without CDN credentials uploads are `503`.

## Design

Contract: [media.openapi.yaml](../../../contracts/media/media.openapi.yaml). Feature card: [Media library](../../../features/media/media-library.md). Data: [Media](../../../architecture/data-model/media-library.md). Runbook: [CDN sync](../../../operations/jobs/media-cdn-sync-runbook.md).

- Storage: CDN Express over FTP (FluentFTP), a dedicated FTP user whose home is the CDN root; password in GCP secret `cdn-media-ftp-password` (`MediaStorage__Ftp__Password`). Without credentials the API answers `503` on writes and the sync job fails fast.
- Public URL = `MediaStorage:PublicBaseUrl` + `/` + key, built at read time (a host change needs no data change).
- Dimensions come from the image header (SkiaSharp codec, no full decode). The sync job does not download files, so imported files have no dimensions until stage 4.
- Upload order: upload to the CDN first, then save the row; if saving fails, the new CDN file is deleted. Replace deletes the old CDN file after the row is saved (a failure there leaves an orphan file, logged, never a broken link).

## Planned file map (stage 1)

| Path | Role | Status |
| --- | --- | --- |
| `src/Media/Media.Domain/Folders/MediaFolder.cs` | folder aggregate (R-1) | implemented |
| `src/Media/Media.Domain/Files/MediaFile.cs` | file aggregate (R-3, R-5) | implemented |
| `src/Media/Media.Domain/MediaErrors.cs` | domain errors | implemented |
| `src/Media/Media.Application/Contracts/Storage/IMediaStorage.cs` | CDN port: upload, delete, list | implemented |
| `src/Media/Media.Application/Contracts/Images/IImageInspector.cs` | image type and size port | implemented |
| `src/Media/Media.Application/Contracts/Persistence/*` | repository and read context | implemented |
| `src/Media/Media.Application/Features/Folders/{GetTree,Create,Update,Delete}/` | folder use cases + validators (Update = rename + move) | implemented |
| `src/Media/Media.Application/Features/Files/{GetList,GetById,Upload,Replace,Update,Move,Delete}/` | file use cases + validators (Update = rename + move one) | implemented |
| `src/Media/Media.Application/Features/{Files,Folders}/Common/` | DTOs, specs, folder rules, upload store | implemented |
| `src/Media/Media.Application/Jobs/CdnSync/SyncCdnMediaJob.cs` | CDN sync job (R-8) | implemented |
| `src/Media/Media.Infrastructure/DataBase/MediaDbContext.cs` | schema `media`, own history | implemented |
| `src/Media/Media.Infrastructure/Storage/FtpMediaStorage.cs` | FTP adapter | implemented |
| `src/Media/Media.Infrastructure/Images/SkiaImageInspector.cs` | image header reader | implemented |
| `src/Media/Media.Infrastructure/Storage/{MediaUrlBuilder,UnconfiguredMediaStorage,MediaStorageOptions}.cs` | public URL, 503 fallback, options | implemented |
| `src/Media/Media.Api/Controllers/AdminMediaFoldersController.cs` | `/api/admin/media/folders` | implemented |
| `src/Media/Media.Api/Controllers/AdminMediaFilesController.cs` | `/api/admin/media/files` | implemented |
| `src/Bootstrapper/Host.Jobs/Host.Jobs/Program.cs` | `--job=media-cdn-sync [--dry-run]` | changed |
| `.github/workflows/deploy-cloudrun.yml` | FTP secret for the API, Cloud Run job `media-cdn-sync` | changed |

## Rollout and rollback

Before merging: the owner creates the FTP user and the GCP secret `cdn-media-ftp-password` with Secret Accessor for the deploy service account (the deploy fails without it). Deploy runs `--job=migrate` (creates schema `media`). Then the owner runs the Cloud Run job `media-cdn-sync` once with `--dry-run`, reads the report, and runs it without the flag. Rollback: the library has no consumers until stage 3; reverting drops schema `media` and leaves CDN files untouched (uploaded `library/` files stay as orphans).

## Tasks and traceability

| Task | Scenarios | Evidence |
| --- | --- | --- |
| TS-1 Domain, Application, validators | R-1…R-7, SC-3…SC-7 | `UnitTests.Media` (4 passed) |
| TS-2 Infrastructure: schema, FTP, image inspector | R-3, R-4, SC-1 | `MediaPostgresTests` on PostgreSQL 18 (2 passed); FTP adapter not run against the real CDN yet |
| TS-3 CDN sync job and deployment | R-8, SC-2 | `MediaPostgresTests.CdnSync_*`; real dry run after deploy |
| TS-4 HTTP, policies, contract | R-9, SC-8 | `MediaApiTests` (7 passed); architecture tests (10 passed); `ApiContractTests` |

## Stage 1 structure check

Folders hold at most 5 production files; each handler, validator, and DTO file has one role. `MediaFolderRules` (Folders/Common) is shared by file handlers for the folder-exists check, which keeps one place for folder lookups. `SyncCdnMediaJob` classifies, maps directories to folders, and registers in one class of about 140 lines; split the directory mapping out if stage 3 adds rules to it. Residual risk: `FtpMediaStorage` is verified only by the same FluentFTP calls the avatar upload uses in production; the first real check is the dry run.
