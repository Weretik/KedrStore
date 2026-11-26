# Platform 010: «SEO-огляд»

**Status:** implemented 2026-11-26. **Modules:** Catalog, Content. **Feature card:** [«SEO-огляд» data](../../../features/platform/seo-review.md).

## Goal

SEO stage 3 (owner, 2026-11-26): one admin page that lists the site pages whose search texts need work: no text, no description, a title too long, the same title twice. Like the admin «Огляд», each module reviews its own pages and the admin joins the lists.

**Out of scope:** the eleven fixed site pages (their texts default in the site code, so nothing can be missing), product pages (they take the model's texts), fixing texts from the review page (it links to the editors).

## Rules

- R-01 **Pages.** Catalog: categories shown on the site (not hidden, not under the Cosmos root) and not closed from search ([catalog/011](../../catalog/011-site-search-settings/README.md) R-01), and accepted models with on-site products in them, from the public model catalogue (up to two minutes old). Content: published articles. Each page is reviewed once per language (`uk`, `ru`).
- R-02 **Issues** ([CatalogSeoChecks](../../../../../src/Modules/Catalog/Catalog.Application/Features/Overview/Queries/GetCatalogSeoReview/CatalogSeoChecks.cs), [ContentSeoChecks](../../../../../src/Modules/Content/Content.Application/Features/Overview/Queries/GetContentSeoReview/ContentSeoReview.cs)):
  - `noText`: a category without its body text, a model without its description (articles always have a body);
  - `noDescription`: no own search description (categories: meta description; models: search description; articles: meta description, the site then uses the excerpt);
  - `longTitle`: the title over 70 characters (models: their search title; articles: the search title, else the title; category search titles are limited to 70 already);
  - `longDescription`: a model search description over 160 characters;
  - `duplicateTitle`: the same title (trimmed, case-insensitive) on two pages of one kind and language.
- R-03 **Freshness.** Texts are read fresh, so a saved text leaves the review on the next load.
- R-04 **Access.** `GET api/admin/catalog/seo-review` under `CanManageSiteCatalog` (Seo, Admin), `GET api/admin/content/seo-review` under `CanManageContent` (Manager, Seo, Admin). Both return `{kind, id, name, language, issues[]}` for pages with at least one issue. Read-only, cheap signed-in calls under the global soft limit.

## Scenarios

- SC-01 The SEO specialist opens «SEO-огляд» and sees «Замки», uk: no text, no description.
- SC-02 They write the Ukrainian text and description of «Замки»; on reload the uk row is gone, the ru row stays.
- SC-03 A category closed from search leaves the review with its models.

## Load

Catalog: one cached catalogue read plus two text queries filtered by id (a few hundred categories and models); Content: one projection of published article texts. No loops of queries.

## Evidence

Integration `CatalogSeoReviewApiTests` (access, issues per language, a saved text leaves, closed category leaves) and `ContentSeoReviewApiTests` (issues, drafts left out, access) pass.

## Final audit (2026-11-26)

Reviewed with `content/013-site-redirects` (see its final audit for step 0 and the method). Both queries are read-only projections without tracking and without queries in a loop; access matches the edit rights of each kind of page; each module reviews only its own data and exposes its own endpoint. No findings.
