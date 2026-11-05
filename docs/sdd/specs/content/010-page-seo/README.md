# Content 010: search texts of the site pages edited in the admin

**Status:** implemented 2026-11-05. **Owner module:** Content. **Feature card:** [Search texts of the site pages](../../../features/content/page-seo.md).

## Goal

The owner asked (2026-11-05) to edit the search title and description of the site pages that exist once (home, catalog, «Співпраця», delivery, returns, contacts, gallery, articles, requisites, offer, privacy) in the admin, without a code change.

**Out of scope:** categories (their SEO texts live in Catalog), products and articles (their own title and excerpt), search and checkout (never indexed), Open Graph images.

## Rules

- R-001 Table `content."PageSeo"` has one row per edited page, keyed by the page name from `SitePages`; values `TitleUk`, `TitleRu` (≤ 120), `DescriptionUk`, `DescriptionRu` (≤ 300) are optional, trimmed, and a blank value is stored as null. An unknown page is refused.
- R-002 Where a value is null the site keeps the text it has in code, so the table starts empty and the migration inserts nothing.
- R-003 `GET /api/content/{lang}/page-seo` is public and lists, in menu order, only pages with a title or description in that language; `lang` other than `uk`/`ru` is 400.
- R-004 `GET /api/admin/content/page-seo` (`CanManageContent`) lists every page in menu order with empty values for pages never saved; `PUT /api/admin/content/page-seo/{page}` replaces the four values of one page, creating its row on the first save, and records who saved it (`updatedBy`, never public).

## Scenarios

- SC-001 A manager sets the Ukrainian title of «Співпраця»; the site uses it, and the Russian page keeps its code text.
- SC-002 Clearing a value returns that text to the site default.
- SC-003 An unknown page or a text over its limit is refused with a field error and nothing is stored.

## Data and rollout

Table `content."PageSeo"`, migration `20261105005706_AddPageSeo` (no rows). Deploy the backend first; the site treats a failed read as «no texts» and keeps its own. Rollback: Down drops the table.

## Evidence

Unit `PageSeoTests` (3), integration `IntegrationTests.Content` (91, with `PageSeoApiTests`: menu-order list, access by role, create then replace, public language filter, refusals) and architecture tests (10) pass.
