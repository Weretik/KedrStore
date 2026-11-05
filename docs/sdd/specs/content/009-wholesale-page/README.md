# Content 009: page «Співпраця» edited in the admin

**Status:** implemented 2026-11-05. **Owner module:** Content. **Feature card:** [Page «Співпраця»](../../../features/content/wholesale-page.md).

## Goal

The owner asked (2026-11-05) to move the texts of the site page «Співпраця» (wholesale offer) from site code into the admin, like the other site content.

**Out of scope:** buttons, link labels, breadcrumbs, and category cards (site design, they stay in code); contact values (they come from [company contacts](../002-company-contacts/README.md)); a different number of order steps (the design has three).

## Rules

- R-001 One row `content."WholesalePages"` holds both languages; each language is one jsonb document with: hero title and lead, contact card title and note, assortment title and intro, company title and text, «Підбір» and «Складська програма» titles and texts, steps title and exactly three steps (title, text), questions title and 1–12 questions (question, answer), closing title. All are plain text, trimmed, required, with the limits in the contract.
- R-002 `GET /api/content/{lang}/wholesale` is public and returns one language; `lang` other than `uk`/`ru` is 400.
- R-003 `GET/PUT /api/admin/content/wholesale` (`CanManageContent`, Manager or Admin) read and replace both languages at once; the PUT needs exactly one `uk` and one `ru` text and records who saved it (`updatedBy`, never shown publicly).
- R-004 The migration `AddWholesalePage` inserts the texts the site kept in code, so the page looks the same right after the deploy.

## Scenarios

- SC-001 The site renders «Співпраця» from the API in Ukrainian and Russian.
- SC-002 A manager changes the hero title and adds a fourth question; the site shows them after its cache refresh.
- SC-003 A text with two steps, an empty field, or only one language is refused with a field error and nothing changes.

## Data and rollout

Table `content."WholesalePages"` (check `Id = 1`), migration `20261105000100_AddWholesalePage` (table and seed). Deploy the backend first: the current site still uses its code texts, and the new site falls back to them while the API is unreachable. Rollback: Down drops the table.

## Evidence

Unit `WholesalePageTests` (3), integration `IntegrationTests.Content` (84: migration seed in both languages, update round-trips jsonb and a second row is refused, HTTP access by role, validation) and architecture tests (10) pass.
