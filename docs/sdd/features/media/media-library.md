# Media library

**Static code check:** 2026-11-04. **Module:** Media.

The library of pictures on the CDN (`images-kedr.cdn.express`): virtual folders, upload, replace, rename, move, and delete, plus a job that registers the files that already sit on the CDN. Folders live only in the database, so moving anything never changes a CDN URL. Catalog will link product photos and schemes to library files by ID (stage 3 of the [specification](../../specs/media/001-media-library/README.md)).

## Structure and relationships

[AdminMediaFoldersController](../../../../src/Media/Media.Api/Controllers/AdminMediaFoldersController.cs) and [AdminMediaFilesController](../../../../src/Media/Media.Api/Controllers/AdminMediaFilesController.cs) dispatch to one handler per use case. [MediaFolder](../../../../src/Media/Media.Domain/Folders/MediaFolder.cs) is a named node with an optional parent; [MediaFile](../../../../src/Media/Media.Domain/Files/MediaFile.cs) holds the CDN key, display name, type, size, and optional dimensions. Folder checks that need other folders (name clash among siblings, parent exists, no cycle) live in [MediaFolderRules](../../../../src/Media/Media.Application/Features/Folders/Common/MediaFolderRules.cs).

Uploads go through [MediaUploadStore](../../../../src/Media/Media.Application/Features/Files/Common/MediaUploadStore.cs): the [image inspector](../../../../src/Media/Media.Infrastructure/Images/SkiaImageInspector.cs) reads the real type and size from the picture header, the file is stored unchanged at `library/<yyyy>/<MM>/<guid>.<ext>` through [FtpMediaStorage](../../../../src/Media/Media.Infrastructure/Storage/FtpMediaStorage.cs), then the row is saved. Replace keeps the file ID, points it at a new key, and deletes the old CDN file. Without CDN credentials `UnconfiguredMediaStorage` is registered and writes answer `503`.

[SyncCdnMediaJob](../../../../src/Media/Media.Application/Jobs/CdnSync/SyncCdnMediaJob.cs) (`--job=media-cdn-sync [--dry-run]`) lists the CDN without `avatars/` and dot service directories such as `.thumbs/`, skips known keys, reports unknown files in `library/` as orphans, and registers other pictures: `products` → folder «Товари», `product-scheme` → «Схеми», deeper directories → subfolders.

Access: everything needs `CanManageMedia` (Manager, Admin); file and folder delete also need `RequireAdminRole`.

## Code structure

```text
src/Media/
├── Media.Api/
│   ├── Controllers/AdminMediaFoldersController.cs  # /api/admin/media/folders
│   ├── Controllers/AdminMediaFilesController.cs    # /api/admin/media/files
│   └── Requests/MediaRequests.cs                   # request bodies
├── Media.Application/
│   ├── Contracts/                                  # persistence, storage (CDN, URLs), image inspector ports
│   ├── Features/Folders/                           # GetTree, Create, Update (rename + move), Delete
│   ├── Features/Files/                             # GetList, GetById, Upload, Replace, Update, Move, Delete
│   └── Jobs/CdnSync/                               # SyncCdnMediaJob, options, report
├── Media.Domain/                                   # MediaFolder, MediaFile, errors
└── Media.Infrastructure/
    ├── DataBase/MediaDbContext.cs                  # schema media, own migration history
    ├── Storage/                                    # FTP storage, URL builder, options
    ├── Images/SkiaImageInspector.cs
    └── Migrations/*_InitialMedia.cs
```

## Structure diagram

```mermaid
flowchart LR
    A["Admin controllers"] --> B["Folder / file handlers"]
    B --> C["MediaFolder, MediaFile"]
    B --> D["IMediaStorage (FTP)"]
    B --> E["IImageInspector"]
    J["Host.Jobs media-cdn-sync"] --> K["SyncCdnMediaJob"]
    K --> D
    K --> C
    C --> F[("schema media")]
    D --> G["CDN Express"]
```

## Data

Tables `media."MediaFolders"` and `media."MediaFiles"`; see the [database map](../../architecture/data-model/media-library.md).

## Tests

- Unit: [MediaDomainTests](../../../../tests/UnitTests/Media/MediaDomainTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Media"`.
- Integration: [HTTP, access, uploads, folders](../../../../tests/IntegrationTests/Media/MediaApiTests.cs), [PostgreSQL schema, CDN sync, list queries](../../../../tests/IntegrationTests/Media/MediaPostgresTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Media"`.

## Related sources

- [Specification](../../specs/media/001-media-library/README.md)
- [Contract](../../contracts/media/media.openapi.yaml)
- [CDN sync runbook](../../operations/jobs/media-cdn-sync-runbook.md)
- Used by [product photos and schemes](../catalog/product-media.md) through `Media.Contracts` (`IMediaFileDirectory`, `IMediaFileChangeListener`); replace and delete call the listeners after the save.
