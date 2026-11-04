# Content 005: site gallery

**Status:** implemented 2026-11-04. **Owner module:** Content (photos from Media). **Feature card:** [Site gallery](../../../features/content/gallery.md).

## Goal

The gallery page listed seven YouTube videos kept in site code with local poster files, and photos had no way in. The owner asked (2026-11-04) for a gallery edited in the admin: photos picked from the media library, videos added by pasting a YouTube link, «зроби зручно». Plan [admin-site-content](https://github.com/Weretik/kedr-web/blob/main/docs/ideas/admin-site-content.md), stage 4.

**Out of scope:** albums or categories, drafts and scheduled publishing, videos from other hosts, uploading files from the gallery page itself (uploads go through the media library), automatic sync with the YouTube channel.

## Rules

- R-001 A card is a photo (`MediaFileId` of a media-library file) or a YouTube video (11-character ID), never both (check constraint). A picture or video appears once (filtered unique indexes).
- R-002 Titles (≤150) and captions (≤500) in `uk` and `ru` are optional; blank is stored as none.
- R-003 Links accepted: `youtube.com/watch?v=`, `youtu.be/`, `/shorts/`, `/embed/`, `/live/`, `m.`/`music.`/`youtube-nocookie.com`, or the bare ID; anything else 400.
- R-004 Adding photos takes up to 100 IDs, appends them in order, skips ones already in the gallery; a file not in the library answers 400 and nothing is added.
- R-005 Reorder takes every current card ID once; a list that does not match answers 409.
- R-006 Content never reads Media tables: it stores the ID and reads URL, name and size through `Media.Contracts.IMediaFileLookup`. A photo whose file was deleted is hidden on the site and flagged `fileMissing` for the admin.
- R-007 The paste preview asks YouTube oEmbed for the title; on any failure the title is `null` and the manager types it.
- R-008 `GET /api/content/{lang}/gallery` is anonymous; admin endpoints need `CanManageContent` (Manager or Admin).
- R-009 The migration inserts the seven videos the site had, in its order, with their uk/ru titles.

## Scenarios

- SC-001 After the migration the site gets the seven videos in the former order with YouTube posters.
- SC-002 A Manager pastes `https://youtu.be/…?si=…`, sees the poster and title, adds it; pasting it again answers 409.
- SC-003 A Manager picks photos in the media library; they appear at the end; picking one again does not duplicate it.
- SC-004 A Manager reorders, edits a caption and removes a card; the site shows the new order and texts.
- SC-005 A library file is deleted: the site hides that photo, the admin marks it.
- SC-006 Anonymous and `User` requests to the admin endpoints get 401/403.

## Data and rollout

Table `content."GalleryItems"` (identity id, kind as string, position index). Migrations `AddGallery` (schema) and `SeedGallery` (seven videos). New project `Media.Contracts` with `IMediaFileLookup`, implemented in Media.Infrastructure and used by Content.Infrastructure. The deploy runs the migrations; the backend must be deployed before the site reads the new endpoint. Rollback: Down of the seed deletes the seven rows, Down of the schema drops the table.

## Evidence

Unit `UnitTests.Content` (60) and integration `IntegrationTests.Content` (54, HTTP on InMemory with fake library and YouTube, migration seed and unique index on PostgreSQL 16) pass; `MediaPostgresTests` lookup test passes; architecture tests (10) and `ApiContractTests` (13) pass.
