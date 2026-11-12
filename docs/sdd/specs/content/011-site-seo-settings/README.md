# Content 011: site SEO settings edited in the admin

**Status:** implemented 2026-11-12. **Owner module:** Content. **Feature card:** [Site SEO settings](../../../features/content/site-seo-settings.md).

## Goal

The owner asked (2026-11-12) for the Kedr admin page «Налаштування SEO сайту», as Brevi has: the SEO specialist pastes the Google Analytics 4 and Tag Manager ids, the Google Search Console and Bing Webmaster verification codes, and picks the picture for link previews; the site places them on every page without a code change.

**Out of scope:** connecting the services themselves (the SEO specialist's job), the search texts of pages (spec 010), DNS verification already done for classkedr.com.ua (it keeps working: an empty field adds nothing).

## Rules

- R-001 Table `content."SiteSeoSettings"` has exactly one row (`Id = 1`, check constraint), inserted empty by the migration; it is only replaced.
- R-002 `GoogleAnalyticsId` matches `G-[A-Z0-9]{4,15}`, `GoogleTagManagerId` matches `GTM-[A-Z0-9]{4,12}`; both are trimmed and upper-cased. Verification codes match `[A-Za-z0-9_-]{8,100}`; when a whole meta tag is pasted its `content` is kept. A blank value is stored as null and removes that code from the site.
- R-003 `ShareImageMediaFileId` is a media-library file ID without a cross-module key; a newly chosen file must exist in the library. A file deleted later is shown to the admin as missing and is hidden from the site, which then keeps its own picture.
- R-004 `GET /api/content/seo-settings` is public and never returns who edited; `GET`/`PUT /api/admin/content/seo-settings` need `CanManageContent` (Manager, Seo, or Admin) and record `updatedBy`.

## Scenarios

- SC-001 The SEO specialist pastes the GA4 id and the Google meta tag; every site page gets the GA4 script and the `google-site-verification` meta tag.
- SC-002 Clearing a field removes that code from the site.
- SC-003 A code of another shape, or a picture not in the library, is refused with a field error and nothing changes.
- SC-004 With no picture chosen the site keeps its own link-preview picture.

## Data and rollout

Table `content."SiteSeoSettings"`, migration `20261112121748_AddSiteSeoSettings` (one empty row). Deploy the backend first; the site treats a failed read as «no codes». Rollback: Down drops the table.

## Evidence

Unit `SiteSeoSettingsTests` (7) and integration `SiteSeoSettingsApiTests` (10: public read, access by role, save then site read, missing picture, refusals) pass.
