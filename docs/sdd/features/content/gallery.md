# Site gallery

**Static code check:** 2026-11-04. **Module:** Content (photos read from Media).

The cards of the site gallery page: photos picked from the media library and YouTube videos added by pasting a link, each with an optional title and caption in Ukrainian and Russian, shown in the order the admin sets. Managers and Admins edit it; the site reads it anonymously. The seven videos the site kept in code were moved into the database by a data migration.

## Structure and relationships

[GalleryItem](../../../../src/Content/Content.Domain/Gallery/GalleryItem.cs) is the aggregate: a photo holds a `MediaFileId` (a value, not a foreign key), a video holds the 11-character YouTube ID, and both hold a position and the optional texts ([GalleryItemTexts](../../../../src/Content/Content.Domain/Gallery/GalleryItemTexts.cs): empty becomes none, title up to 150, caption up to 500). [YoutubeVideoId](../../../../src/Content/Content.Domain/Gallery/YoutubeVideoId.cs) reads the ID from watch, youtu.be, shorts, embed and live links or the bare ID and rejects other hosts.

Photos come from the Media module only through [IMediaFileLookup](../../../../src/Media/Media.Contracts/Files/IMediaFileLookup.cs) (Media.Contracts): Content.Application asks its own port `IGalleryImageReader`, and [GalleryImageReader](../../../../src/Content/Content.Infrastructure/Gallery/GalleryImageReader.cs) adapts the lookup. A photo whose library file was deleted is hidden on the site and marked `fileMissing` in the admin. A video poster is YouTube's `hqdefault.jpg`. The paste preview asks YouTube's public oEmbed for the title ([YoutubeOEmbedClient](../../../../src/Content/Content.Infrastructure/Gallery/YoutubeOEmbedClient.cs), 5 s timeout, `null` on any failure).

Handlers live in `Features/Gallery/{Public,Admin}`; every admin change answers with the whole gallery ([GalleryAdminList](../../../../src/Content/Content.Application/Features/Gallery/Common/GalleryAdminList.cs)):

- add photos — up to 100 IDs, appended in order, already present ones skipped, an unknown file 400 with nothing added;
- add video — 409 when the video is already there;
- update texts, delete (the picture stays in the library);
- reorder — the list must hold every current card once; a stale list answers 409.

## Code structure

```text
src/Media/Media.Contracts/Files/IMediaFileLookup.cs   # read port other modules use (URL, file name, size)
src/Media/Media.Infrastructure/Lookup/MediaFileLookup.cs
src/Content/
├── Content.Api/
│   ├── Controllers/GalleryController.cs         # public GET {lang}/gallery
│   ├── Controllers/AdminGalleryController.cs    # list, photos, videos, youtube-preview, items/{id}, order
│   └── Gallery/GalleryResponses.cs              # responses, requests, mapping
├── Content.Application/
│   ├── Contracts/Gallery/                       # IGalleryImageReader, IYoutubeVideoInfo
│   └── Features/Gallery/
│       ├── Common/                              # DTOs, admin list loader, texts validator, YouTube poster URL
│       ├── Public/GetList/
│       └── Admin/{GetList,AddPhotos,AddVideo,Update,Delete,Reorder,PreviewYoutube}/
├── Content.Domain/Gallery/                      # item aggregate, texts, kind, YouTube ID parser, errors
└── Content.Infrastructure/
    ├── Configurations/GalleryItemConfiguration.cs
    ├── Gallery/                                 # Media lookup adapter, YouTube oEmbed client
    ├── Seed/LegacyGallerySeed.cs                # the seven site videos
    └── Migrations/*_AddGallery.cs, *_SeedGallery.cs
```

## Data

Table `content."GalleryItems"`; see the [database map](../../architecture/data-model/content-articles.md).

## Tests

- Unit: [GalleryItemTests](../../../../tests/UnitTests/Content/GalleryItemTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Content"`.
- Integration: [HTTP, access, add/reorder/edit/delete](../../../../tests/IntegrationTests/Content/GalleryApiTests.cs), [PostgreSQL seed and unique index](../../../../tests/IntegrationTests/Content/GalleryPostgresTests.cs), [Media lookup on PostgreSQL](../../../../tests/IntegrationTests/Media/MediaPostgresTests.cs) — `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Content|FullyQualifiedName~MediaPostgresTests"`.

## Related sources

- [Specification](../../specs/content/005-gallery/README.md)
- [Contract](../../contracts/content/gallery.openapi.yaml)
- [Media library](../media/media-library.md)
