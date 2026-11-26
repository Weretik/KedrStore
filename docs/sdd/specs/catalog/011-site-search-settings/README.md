# Catalog 011: category search settings for the site

**Status:** implemented 2026-11-26. **Owner module:** Catalog. **Feature cards:** [Catalog categories](../../../features/catalog/categories.md), [Public site models](../../../features/catalog/public-site-models.md).

## Goal

The SEO audit of 2026-11-26 found that the section «Товари, виведені з асортименту» is indexed, that category titles carry Russian 1C names on Ukrainian pages, that a category page has no link-preview picture of its own, and that the sitemap has no `lastmod`. The owner decided (2026-11-26): category names are admin fields that win over 1C, and the 1C programmer is not asked to change anything.

**Out of scope:** texts (the SEO specialist writes them), 301 redirects and the «SEO-огляд» page (stage 3), the site pages themselves (kedr-web).

## Rules

- R-01 **«Закрити від пошуку».** `ProductCategory.IsClosedFromSearch`, admin-owned. Closing or opening a category does the same to every subcategory, deleted ones included ([CategorySearchClosing](../../../../../src/Modules/Catalog/Catalog.Domain/Categories/CategorySearchClosing.cs)); parents keep theirs. The 1C sync never changes it and gives a new category its parent's value. The site keeps showing a closed category and its models; it gives them `noindex, follow` and leaves them out of the sitemap. The public tree, the category page and the model page carry `isClosedFromSearch`.
- R-02 **Site names.** `SiteNameUk` and `SiteNameRu` (up to 100 characters each, blank is none) win over the 1C name and the configured short name: `ShortNameUk`/`ShortNameRu`, which every reader uses, always hold the shown name, and the sync keeps the admin's name. Clearing a name restores the 1C/configured name at once through `CategoryPresentationResolver`.
- R-03 **Link-preview picture.** `ShareImageMediaFileId` is a media-library file by plain value (no cross-module key). A newly chosen file must be in the library (400 `shareImageMediaFileId` otherwise); a file deleted later gives no address, and the site keeps its own picture. The admin by-id read, both commands, and the public category page return `shareImageUrl`.
- R-04 **Change dates for the sitemap.** The public tree carries the later of the category's and its content's change in that language as `updatedAt`; the category page carries the later of the category and its content in that language; a model card carries the later of the accepted model and its texts (`null` for a product shown as a model of its own).
- R-05 **Access and caches.** `PUT api/categories/{id}/search-settings` and `PUT api/categories/{id}/site-names` need `CanManageSiteCatalog` (Admin or Seo); the cheap signed-in calls stay under the global soft rate limit. Both drop the category tree and site models caches; the public model catalogue follows within its 2-minute lifetime. Repeating a request changes nothing.

## Scenarios

- SC-01 The SEO specialist closes «Товари, виведені з асортименту»; its subcategories close too, and the site marks the pages `noindex` and drops them from the sitemap.
- SC-02 They write «Дверні замки» / «Дверные замки» for a category; the tree, breadcrumbs and titles show them, the next 1C sync keeps them; a blank name goes back to the 1C name.
- SC-03 They pick a library picture for a category; the category page's link preview shows it; a file outside the library is refused.

## File-role map

| Role | File |
| --- | --- |
| Domain rule | `Catalog.Domain/Categories/ProductCategory.cs` (fields, `SetSiteNames`, `SetClosedFromSearch`, `SetShareImage`), `CategorySearchClosing.cs`, `CategoryErrors.cs` |
| Commands | `Features/Categories/Commands/SetCategorySearchSettings/`, `SetCategorySiteNames/` |
| Reads | `GetAdminCategoryByIdHandler`, `GetCategoryBySlugHandler`, `Shared/Tree/CategoryTree.cs`, `Shared/CategoryShareImage.cs`, `SiteModels/Shared/Public/*` |
| Sync | `OneCSync/Jobs/SyncOneCCategoryJob.cs` (inherits the parent's search mark) |
| Endpoints | `Catalog.Api/Endpoints/Categories/SetAdminCategorySearchSettings/`, `SetAdminCategorySiteNames/` |
| Data | migration `20261126123144_CategorySearchSettings`, `ProductCategoryConfiguration` |

## Data and rollout

Four nullable or defaulted columns on `catalog."ProductCategories"`; nothing is rewritten, so the previous release keeps working. Deploy the backend before kedr-web; the site treats a missing field as «open, no picture, no date». Rollback: Down drops the columns.

## Evidence

Unit `CategorySearchSettingsTests` (6). Integration `CategorySearchSettingsApiTests` (6: access, closing with subcategories, site names and their return to 1C, picture, tree change date with a text edit), `PublicSiteModelsApiTests` (closed model page, no date for a product of its own), and the existing category, sync and visibility tests pass.

## Final audit (2026-11-26, backend-code-audit, scope: this change and content/012)

Quality targets read first: reads P95 328 ms against 1 s, writes P95 1 159 ms against 2 s (2026-11-19). The change adds one content query to the cached tree build and one media lookup to the category page only when a picture is set, so no target moves.

| ID | Severity | Finding | Status |
| --- | --- | --- | --- |
| AF-01 | Medium | A contacts save without `store` (an admin released before this change) cleared the owner's shop. | Fixed: a save without `store` keeps the shop (`CompanyContacts.Apply`), test `SaveWithoutAStore_KeepsTheShop_*`. |
| AF-02 | Low | The public tree's `updatedAt` ignored text edits, so the sitemap `lastmod` of a category missed them. | Fixed: `ICategoryStatsReader.LoadContentChangesAsync` feeds the later date into the tree, test `PublicTree_TakesTheLaterOfTheCategoryAndItsTextChange_R04`. |
| AF-03 | Info | The public model catalogue is not dropped by the new commands; the model page's `isClosedFromSearch` follows within its 2-minute lifetime, as every other admin change does. | Accepted, documented in R-05. |

Checked without findings: module boundaries (Catalog reads the picture through `IProductMediaLibrary`; Content's shop is an owned value of its own aggregate), folder axiom (architecture tests 74/74), one `SaveChanges` per command and repeats change nothing (both PUTs replace state), rate limits (signed-in cheap calls under the global soft limit; `RateLimitCoverageTests` pass), OpenAPI rules (`OpenApiDocumentRulesTests` pass), migrations add nullable or defaulted columns only, so the previous release keeps working.
