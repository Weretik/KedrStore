# Content 008: article cover on the CDN, chosen before the first save

**Status:** implemented 2026-11-04. **Owner module:** Content (uses Media through `Media.Contracts`). **Feature card:** [Site articles](../../../features/content/articles.md).

## Goal

The owner asked (2026-11-04) that a new article gets its cover right away, not after a first save, and that article pictures go to the CDN into their own `articles` folder.

**Out of scope:** moving the six legacy covers (bytes in `content."ArticleCovers"`) to the CDN; they keep working and move when an editor picks a new cover. Dropping `ArticleCovers` waits for that.

## Rules

- R-001 `POST /api/admin/articles/covers` (`CanManageContent`) takes a JPEG/PNG/WebP up to 10 MB and at least 600 px wide, turns it into WebP at most 1600 px wide (the existing cover processor), and stores it through `Media.Contracts.IMediaFileUploader`: CDN key `articles/<guid>.webp`, library folder «Статті» at the root (created when missing). It answers `{ mediaFileId, url }`; no article changes.
- R-002 Create and update take `coverMediaFileId` (a file from R-001 or any library picture) and `removeCover`. A new file must exist in the library (else 400 `coverMediaFileId`); with neither field the cover stays (so an older admin never wipes it); both together are 400 `removeCover`.
- R-003 `Article.CoverMediaFileId` is a plain value (no reference to Media tables). Setting or removing it drops a legacy cover (`CoverVersion` and its `ArticleCovers` row).
- R-004 Responses carry `coverUrl` (absolute CDN address, read through `IMediaFileLookup`) and, only for a legacy cover, `coverPath`. Admin responses also carry `coverMediaFileId`. A deleted library file gives `coverUrl: null`, so the site shows no picture instead of a broken one.
- R-005 The old `PUT/DELETE /api/admin/articles/{id}/cover` are removed; the public legacy `GET /api/content/articles/{id}/cover` stays.

## Scenarios

- SC-001 A manager opens «Нова стаття», uploads a picture, fills the texts and presses «Створити»: the article is created with its cover in one save.
- SC-002 A manager picks an existing library picture as the cover.
- SC-003 Replacing a legacy cover removes its bytes; the site then uses the CDN address.
- SC-004 Without CDN credentials the upload answers 503.

## Data and rollout

Migration `AddArticleCoverMediaFile` adds nullable `CoverMediaFileId bigint` to `content."Articles"`. Deploy the backend first: the current admin still saves articles (it sends neither cover field), only its cover button stops until the new admin is merged. Rollback: Down drops the column; covers picked meanwhile stay in the library.

## Evidence

Unit `UnitTests.Content` (70), integration `IntegrationTests.Content` (74: upload then create with the cover, legacy bytes removed on replace, unknown file refused, remove) and `IntegrationTests.Media` (uploader keys, one «Статті» folder, not a picture, no CDN) pass; architecture tests (10) pass.
