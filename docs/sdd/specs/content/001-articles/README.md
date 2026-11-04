# Content 001 — Site articles in the database

**Status:** implemented 2026-11-04 (branch `claude/project-thread-mr1vpf`). **Module:** Content (new).

## Goal and boundaries

The owner asked (2026-11-04) for a separate bounded context for site content, starting with articles: the current six site articles move into the database, the site reads them from the API, and staff add and edit them in the admin.

In scope: article storage, public reads, Manager/Admin CRUD, cover pictures, migration of the six legacy articles. Out of scope: other site texts (offer, phone, requisites — a later audit proposes them), comments, authors, tags, scheduled publishing.

## Module decision

A new module `Content` (Domain, Application, Infrastructure, Api) under the [module rules](../../../standards/module-rules.md): articles have no business rule shared with Catalog, Sales, or Identity. Alternatives considered: Catalog (rejected: catalog is 1C-driven product data) and the site repository only (rejected: the owner wants admin editing). Tables live in schema `content` with their own migration history; no cross-module references. The KMU-2010 transliterator moved from Catalog to `BuildingBlocks.Application/Text` so both modules build slugs the same way.

## Rules

- R-1: an article has one slug (`a-z0-9` words joined by single hyphens, ≤120) shared by both languages, a publication date, a published flag, and exactly one `uk` and one `ru` text (title ≤200, excerpt ≤300, body ≤50 000).
- R-2: only published articles are listed or opened publicly; a draft or missing slug is `404`.
- R-3: on create an empty slug is built from the Ukrainian title (KMU-2010, ≤80, `-2`, `-3`… on a clash); a given slug must be free. On update the slug is required and must not belong to another article.
- R-4: the body is sanitized on save to p, br, strong, em, ul, ol, li, h2, h3, blockquote, a[href http/https/site path]; script-like tags lose their content.
- R-5: a cover is JPEG/PNG/WebP up to 10 MB, ≥600 px wide; it is stored as WebP ≤1600 px wide with EXIF orientation applied. Every new cover increases `CoverVersion`; responses give `coverPath` with `?v=<version>`, and a matching cover request is cached for a year.
- R-6: delete removes the article and its cover; unpublishing is the reversible way to hide it.
- R-7: the six legacy articles keep ids 1–6, their slugs, date 2026-03-24, and covers; paragraphs become `<p>` blocks and the excerpt is the first sentence, as the site showed them.
- R-8: reads are anonymous; management needs `CanManageContent` (Manager, Admin).

## Scenarios

- SC-1: after `--job=migrate` the database has schema `content` with the six published legacy articles and covers.
- SC-2: the site lists published articles in its language and opens one by slug; drafts are hidden.
- SC-3: a Manager creates an article without a slug and gets a free slug from the Ukrainian title.
- SC-4: a Manager uploads a cover; `coverPath` changes version and the picture is WebP.
- SC-5: an anonymous user gets `401` and a customer `403` on `/api/admin/articles`.

## Design

Contract: [articles.openapi.yaml](../../../contracts/content/articles.openapi.yaml). Feature card: [Site articles](../../../features/content/articles.md). Data: [Content articles](../../../architecture/data-model/content-articles.md).

Covers are stored in PostgreSQL (`bytea`, tens of KB each) instead of the CDN: no new FTP account is needed and the cover version makes caching safe. The seed reads WebP covers embedded in `Content.Infrastructure`.

## Rollout and rollback

Deploy runs `--job=migrate` automatically; `ContentDbContext` migrates after the other modules. No job or manual step is needed. The site and admin switch to the API in kedr-web after this is deployed. Rollback: the site keeps working from its own code until its switch is deployed; reverting the migrations drops schema `content` contents (`SeedLegacyArticles.Down` deletes ids 1–6).

## Tasks and traceability

| Task | Scenarios | Evidence |
| --- | --- | --- |
| TS-1 Domain, Application, validators | R-1…R-6, SC-3 | `UnitTests.Content` (14 passed) |
| TS-2 Infrastructure, schema, seed migration | R-4, R-5, R-7, SC-1 | `IntegrationTests.Content` on PostgreSQL 16 (15 passed) |
| TS-3 HTTP, policy, contract | R-2, R-8, SC-2, SC-4, SC-5 | `ArticlesApiTests`; architecture tests (10 passed) |
