# Content 004: site documents with editions

**Status:** implemented 2026-11-04. **Owner module:** Content. **Feature card:** [Site documents](../../../features/content/legal-documents.md).

## Goal

The offer, privacy, returns and delivery pages lived in site code, so every wording change needed a release and nobody could tell which edition a buyer accepted. They are now edited in the admin with numbered editions and a «Редакція від» date; the site reads the published edition from the API (plan [admin-site-content](https://github.com/Weretik/kedr-web/blob/main/docs/ideas/admin-site-content.md), stage 3). Delivery & payment was added at the owner's request (2026-11-04); the site does not show a date for it.

**Out of scope:** scheduled publishing on a future date, approval by a second person, a diff between editions, restoring an old edition with one click (copy its text into a new draft instead), page chrome such as the hero, summary cards and help blocks, which stay in site code.

## Rules

- R-001 Kinds: `public-offer`, `privacy-policy`, `returns-policy`, `delivery-payment`. An edition holds exactly one `uk` and one `ru` text: title up to 200 characters, body HTML up to 100 000, both required.
- R-002 Status Draft → Published → Archived. At most one draft and one published edition per kind (filtered unique indexes). Only a draft can change or be deleted.
- R-003 A new draft copies the published texts unless texts are sent; 409 when a draft exists.
- R-004 Publishing takes `effectiveFrom` («Редакція від»), not earlier than the edition on the site (400 on `effectiveFrom`). The draft gets the next number; the previous edition is archived in the same save.
- R-005 The body is sanitized on save with the article allow-list (h2, h3, p, br, strong, em, ul, ol, li, blockquote, a[href]). `{site}`, `{email}` and `{phone}` stay as text; the site fills them.
- R-006 `GET /api/content/{lang}/documents/{kind}` is anonymous and answers 404 for an unknown kind or nothing published; admin endpoints need `CanManageContent` (Manager or Admin).
- R-007 The migration inserts edition 1 of each document with the texts the site had, effective 2026-11-03, edited by `seed`.

## Scenarios

- SC-001 After the migration each kind returns edition 1 in both languages with the former site text (privacy keeps `{email}`, returns its h3 subsections, delivery its numbered order steps).
- SC-002 A Manager starts a draft (texts copied), edits it, publishes it from 2026-11-10; the site returns edition 2 at once and the history lists published 2, archived 1.
- SC-003 A second draft answers 409, editing a published edition 409, a date before the current edition 400.
- SC-004 A `<script>` in the body is removed on save.
- SC-005 Anonymous and `User` requests to the admin endpoints get 401/403; an unknown kind 404.

## Data and rollout

Tables `content."LegalDocumentVersions"` (identity id, kind/status as strings, check that a non-draft has number, date and publish time) and `content."LegalDocumentTexts"` (key version + language, cascade). Migrations `AddLegalDocuments` (schema) and `SeedLegalDocuments` (edition 1 from an embedded JSON file, then `setval` on the identity). Deploy runs them automatically; the backend must be deployed before the site reads the new endpoint. Rollback: Down of the seed deletes the seeded rows, Down of the schema drops both tables.

## Evidence

Unit `UnitTests.Content` (41) and integration `IntegrationTests.Content` (46, HTTP on InMemory, migration seed and unique indexes on PostgreSQL 16) pass; architecture tests (10) pass.
