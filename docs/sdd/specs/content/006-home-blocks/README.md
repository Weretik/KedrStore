# Content 006: home page FAQ and brands

**Status:** implemented 2026-11-04. **Owner module:** Content (logos from Media). **Feature card:** [Home page FAQ and brands](../../../features/content/home-blocks.md).

## Goal

The home page FAQ (six questions) and the brand logos were kept in site code. The owner asked (2026-11-04) to make them editable in the admin. Plan [admin-site-content](https://github.com/Weretik/kedr-web/blob/main/docs/ideas/admin-site-content.md), stage 5.

**Out of scope:** the other home texts (hero, partner support, results), brand pages or links, FAQ on other pages (category FAQ stays in the catalog).

## Rules

- R-001 A question has all four texts: question (≤200) and answer (≤2000) in `uk` and `ru`; blank answers 400.
- R-002 A brand has a name (≤100, used as the logo's alternative text) and a logo that is a media-library file (`LogoMediaFileId`, a value, not a foreign key). A logo that is not in the library answers 400.
- R-003 New rows go to the end; reorder takes every current ID of that list once, a stale list answers 409.
- R-004 `GET /api/content/{lang}/home` is anonymous and returns both lists in site order; a brand whose logo file was deleted is left out (the admin sees `logoMissing`).
- R-005 Admin endpoints need `CanManageContent` (Manager or Admin); every change answers with both lists.
- R-006 The migration inserts the six questions the site had, in order. Brands are not seeded: their logos have to be uploaded to the media library first; until a brand is added the site shows its built-in brands.

## Scenarios

- SC-001 After the migration the site gets the six questions in the former order and no brands (it keeps its own).
- SC-002 A Manager adds, edits, reorders and removes questions; the site shows them in the new order.
- SC-003 A Manager adds a brand with a logo picked or uploaded in the media library; the site shows the admin's brands instead of the built-in ones.
- SC-004 A logo file is deleted: the site hides that brand, the admin marks it.
- SC-005 Anonymous and `User` requests to the admin endpoints get 401/403.

## Data and rollout

Tables `content."HomeFaqItems"` and `content."HomeBrands"` (identity id, position index). Migrations `AddHomeBlocks` (schema) and `SeedHomeFaq` (six questions). The picture port `IMediaImageReader` moved to `Contracts/MediaLibrary` so the gallery and brands share it. The deploy runs the migrations; the backend goes before the site. Rollback: Down of the seed deletes the six rows, Down of the schema drops both tables.

## Evidence

Unit `UnitTests.Content` (68) and integration `IntegrationTests.Content` (61: HTTP on InMemory with a fake library, migration seed on PostgreSQL 16) pass; architecture tests (10) pass; no pending model changes.
