# 005 — Stable catalog URLs, soft delete, and category content

**Module:** Catalog. **Status:** Implemented in code 2026-10-30 (not deployed; rollout steps in [design.md](design.md#rollout-and-rollback)). **Date:** 2026-10-30.

Design, data, contracts, file map, and rollout: [design.md](design.md).

## Goal and boundaries

The new public site (kedr-web `apps/site`, step 3 «category page») needs product and category addresses that never break, category pages with their own SEO text and FAQ, and a catalog that does not lose data when 1C temporarily drops an item. Today the 1C import hard-deletes missing products and categories, regenerates every slug from the 1C name on each run, and links products to categories by category name, so one duplicate name stops the whole product import.

**In scope:**

1. Soft delete and restore of products and categories during the 1C import, with a mass-deletion guard.
2. Stable slugs: created once, Ukrainian national transliteration (KMU 2010), no numeric suffix, collision suffix `-2`, slug history, and Admin-only manual edit.
3. Category content per language (SEO title, meta description, H1, rich text, FAQ), edited only by Admin.
4. Public read API additions for the category page: counts, empty/deleted hiding, `404` for unknown categories, canonical slug for old slugs, «in stock first» sorting, an «all products» mode, and min/max price.
5. A more robust and faster category/product import (one load per root, change detection, safe category-name mapping, per-phase timings).

**Out of scope:** the Admin UI and the public site (kedr-web, separate PRs); brand/manufacturer storage and filters (owner: not now); wholesale prices on the public site; sitemap feed and revalidation webhook (site sitemap step); changes on the 1C side; stock and price import logic; the order flow.

Owner decisions are recorded in the [backend analysis](https://claude.ai/artifact/Vy9usNZxR1QH5M1WnqxthJ) («Ответы владельца») and the [category page analysis](https://claude.ai/artifact/9nNizcBesCFPZreWzoq6z3) («Решения владельца»).

## Rules

### Soft delete

- **R-001.** When a product present in the database is absent from a non-empty 1C product response for its root, the import marks it deleted (`IsDeleted=true`, `DeletedAt`) instead of removing the row. Its slug, slug history, translations, prices, photo-check state, and order references stay.
- **R-002.** When a category present in the database is absent from a non-empty 1C category response for its root, the import marks it deleted. Its slug, history, and content stay. Manual groups and the virtual Cosmos category are never marked deleted by this rule.
- **R-003.** A deleted product or category that appears again in 1C with the same 1C ID is restored (`IsDeleted=false`, `DeletedAt=null`) with its previous slug and content; 1C fields are updated as for any existing row.
- **R-004.** Mass-deletion guard: if one run would mark more than 30% of the currently active products (or categories) of a root as deleted, the import marks none of them, logs an error with the counts, and continues with create/update. The existing «empty response → no deletion» behavior stays.
- **R-005.** Deleted products and categories are excluded from every public and Manager-visible read. Products whose category is deleted are excluded from public lists. Admin can list deleted products and categories through an Admin-only filter.

### Slugs

- **R-006.** A slug is generated once, when the product or category is first created by the import. Later imports never change it, even when the 1C name changes.
- **R-007.** Generation: transliterate the Ukrainian name by the KMU 2010 table (for example «Замки» → `zamky`, «Ручки на розетці» → `ruchky-na-rozettsi`), lower-case, keep only `a-z`, `0-9`, and single hyphens, trim to at most 80 characters at a word boundary. An empty result falls back to `category` or `product`. No 1C ID suffix.
- **R-008.** Slugs are unique per entity type across current slugs and slug history. On collision the generator appends `-2`, `-3`, … to the base.
- **R-009.** Every slug change (migration, Admin edit) stores the previous slug in the history for that entity. An old slug always resolves to its entity's current slug; it can never be taken by another entity.
- **R-010.** Admin can set a slug manually or regenerate it from the current name. The value must match `^[a-z0-9]+(-[a-z0-9]+)*$`, be 1–80 characters, and not be used by another entity of the same type (current or history). Only the `Admin` role may change slugs; Manager receives `403`.
- **R-011.** Ukrainian and Russian pages use the same slug.

### Category content

- **R-012.** Each category has optional content per language (`uk`, `ru`): SEO title (≤ 70), meta description (≤ 160), H1 (≤ 120), body text (sanitized HTML, ≤ 20 000 characters), and an ordered FAQ list (each question ≤ 200, answer ≤ 2 000 plain text, at most 20 items). The 1C import never reads or writes content.
- **R-013.** Body HTML is sanitized on save with an allow-list: `p`, `br`, `strong`, `em`, `ul`, `ol`, `li`, `h2`, `h3`, `a[href]` (http, https, relative `/` only). Everything else is removed, not escaped.
- **R-014.** Only `Admin` may create, change, or clear category content; Manager may read it in Admin endpoints. Every save records who changed it and when.

### Public reads

- **R-015.** Category tree: each node includes `productCount` (active, exported-to-site products in the node and its descendants). Deleted categories and categories with `productCount = 0` are omitted, including their subtrees.
- **R-016.** Category by slug: an unknown slug returns `404`; a slug from history returns `200` with `slug` set to the current slug, so the client can redirect; a deleted category returns `404` with a body `{ "redirectSlug": "<nearest active ancestor slug>" }` when such an ancestor exists. A success response adds `productCount`, `minPrice`, `maxPrice` (retail, over active exported products), children with `productCount`, and `content` for the requested language (`null` when none).
- **R-017.** Product list: new sort value `AvailabilityThenName` (in stock first, then name in the requested language, then ID). New optional `availability` parameter: `inStock` (current default behavior) or `all`. When `availability` is supplied it takes precedence over `inStock`. Existing clients that send nothing keep today's behavior. An unknown or deleted `categorySlug` returns `404`; a historical category slug is accepted and filters by that category.
- **R-018.** Product by slug: a slug from history returns `200` with the current `productSlug`. A deleted product returns `404` with `{ "redirectSlug": "<category slug>", "redirectType": "category" }` when its category is active. A success response adds `updatedAt`.
- **R-019.** The numeric 1C product code (`id` in current DTOs) is the article shown to customers; no new SKU field is added.
- **R-020.** All public changes are additive; existing fields, routes, and default behaviors used by the Angular storefront and the mobile app do not change, except that soft-deleted items are no longer returned (they were previously removed rows, so this is not observable as a change).

### Import robustness and speed

- **R-021.** The category import loads all categories of the root (including deleted) in one query and compares in memory.
- **R-022.** A row is written only when a 1C-owned field actually changed; `UpdatedAt` changes only then.
- **R-023.** Product → category mapping by 1C category name: names are compared trimmed and case-insensitive among active categories of the root. A product whose name maps to no category or to several categories keeps its current category if it already exists (and is skipped if new); the run logs one aggregated warning with counts and examples instead of failing the whole root.
- **R-024.** Each root and phase logs its duration and counters: received, created, updated, unchanged, deleted, restored, skipped, guard-blocked. Per-product informational logs are removed.

## Acceptance scenarios

- **SC-001 (R-001, R-005).** Given an active product absent from the next non-empty 1C response; when the import runs; then the product row remains with `IsDeleted=true`, it disappears from public lists and Manager admin lists, and Admin sees it with the deleted filter.
- **SC-002 (R-002, R-005).** Given an active category with content absent from the next 1C category response; when the import runs; then the category is marked deleted, its content and slug stay, and the public tree omits it.
- **SC-003 (R-003).** Given a deleted product or category that returns in 1C; when the import runs; then it is active again with the same slug and content.
- **SC-004 (R-004).** Given a root with 100 active products and a 1C response with 60 of them; when the import runs; then no product is marked deleted, existing products are updated, and an error log reports 40 would-be deletions over the 30% limit.
- **SC-005 (R-006, R-007, R-008).** Given a new category «Замки» and an existing category already using `zamky`; when the import creates it; then its slug is `zamky-2`. Given an existing product renamed in 1C; then its slug is unchanged.
- **SC-006 (R-009, R-016, R-018).** Given a product whose slug changed from `ruchka-a-12345` to `ruchka-a`; when a client requests the old slug; then it receives `200` with `productSlug = ruchka-a`. The same holds for categories.
- **SC-007 (R-010).** Given Admin sets a valid unused slug; then it is saved and the old one moves to history. Given an invalid, too long, or used slug; then `400` with a field error. Given a Manager; then `403`.
- **SC-008 (R-012, R-013, R-014).** Given Admin saves Ukrainian content with `<script>` and an `onclick` attribute in the body; then the stored and returned body contains neither, other allowed markup stays, and `updatedBy`/`updatedAt` are set. A Manager save returns `403`; a Manager read returns `200`.
- **SC-009 (R-015).** Given a branch whose only products are not exported or are deleted; then the public tree omits the branch; other nodes show correct `productCount` including descendants.
- **SC-010 (R-016).** Unknown category slug → `404`. Deleted category with an active parent → `404` with `redirectSlug` of the parent. Active category → `200` with counts, min/max price, children counts, and `content` in the requested language or `null`.
- **SC-011 (R-017, R-020).** A request without `availability` returns only in-stock products as today. `availability=all&sort=AvailabilityThenName` returns in-stock products first. An unknown `categorySlug` returns `404`.
- **SC-012 (R-018).** A deleted product slug returns `404` with `redirectSlug` of its active category.
- **SC-013 (R-021, R-022, R-024).** Given an unchanged 1C response; when the import runs twice; then the second run writes no product or category row, `UpdatedAt` is unchanged, and the log reports all items as unchanged with phase durations.
- **SC-014 (R-023).** Given two active categories with the same name and a product mapped to that name; when the import runs; then the run completes for the root, the product keeps its previous category, and one warning lists the ambiguous name.
- **SC-015 (migration, design §Rollout).** After the migration, every product and category has a KMU 2010 slug without the ID suffix, and each previous slug (`zamki-900002`) resolves to the new one.

## Tasks and traceability

| Task | Scenarios | Depends on | Minimum evidence |
| --- | --- | --- | --- |
| EN-001 — schema and migration: soft delete on categories, `DeletedAt` on products, slug history, category content and FAQ tables, unique slug indexes | SC-001–SC-003, SC-006, SC-008, SC-015 | — | Migration applies to an empty test database (integration fixture); model snapshot reviewed |
| TS-001 — slug generator (KMU 2010, length, collisions) | SC-005 | — | Unit tests: transliteration table, apostrophes, `зг`/`ь`/`щ`, length cut, fallback, collision suffix |
| TS-002 — soft delete, restore, and mass-deletion guard in both import jobs | SC-001–SC-004 | EN-001 | Unit tests on the import diff; integration test for the product job |
| TS-003 — import robustness and speed (one load, change detection, safe name mapping, timings) | SC-013, SC-014 | TS-002 | Unit tests for change detection and ambiguous/missing names; second-run «no writes» integration test |
| TS-004 — slug history and data migration of existing slugs | SC-006, SC-015 | EN-001, TS-001 | Integration test: old slug resolves after migration; uniqueness across history |
| TS-005 — Admin slug edit endpoints (Admin only) | SC-007 | TS-004 | API tests `200`/`400`/`403` |
| TS-006 — category content and FAQ: domain, sanitizer, Admin endpoints | SC-008 | EN-001 | Unit tests for sanitizer and limits; API tests for Admin save, Manager read, Manager `403` |
| TS-007 — public category reads: tree counts/hiding, by-slug `404`/redirect/content/min-max | SC-009, SC-010 | TS-002, TS-004, TS-006 | Query unit/integration tests; API tests for `404` bodies |
| TS-008 — public product reads: `availability`, `AvailabilityThenName`, `404` for category, product redirect info, `updatedAt` | SC-011, SC-012 | TS-002, TS-004 | API tests incl. unchanged default behavior |
| TS-009 — Admin deleted filter for products and categories (Admin only) | SC-001, SC-002 | TS-002 | API tests: Admin sees deleted, Manager filter ignored/`403` |
| TS-010 — contracts, feature cards, operations notes | all | TS-005–TS-009 | OpenAPI updated and `$ref`s valid; `python3 tools/docs/check_doc_links.py` passes; feature cards updated |

Evidence is recorded here after each task. Completed task rows keep their IDs.

### Evidence (2026-10-30)

All tasks EN-001 and TS-001 to TS-010 are done in code. Docker was not available, so PostgreSQL-only tests (Testcontainers) were skipped; the import and API scenarios run on the EF in-memory provider, which also covers query filters and `IgnoreQueryFilters`.

| Check | Result |
| --- | --- |
| `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Catalog"` | 86 passed (TS-001 slug text and allocator, TS-002/TS-003 guard, name map, mapper, stats rollup, domain soft delete and content limits) |
| `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Catalog\|FullyQualifiedName~IntegrationTests.Platform.Api"` | 65 passed, 5 skipped (PostgreSQL); includes SC-001–SC-005, SC-013, SC-014 import tests and SC-006–SC-012 API tests |
| `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` | 10 passed |
| `python3 tools/docs/check_doc_links.py` | valid |
| Migration `20261030181626_StableSlugsSoftDeleteCategoryContent` | generated with `dotnet ef migrations add`; reviewed; not applied to any database |

Not verified here: the migration and `migrate-slugs` on a real PostgreSQL copy (SC-015), and production import timings (R-024). Both belong to the rollout.

Deviations from the first design, all within the accepted rules: one migration instead of two (legacy slugs already end with a unique ID, so the unique indexes apply at once); one shared Admin slug command for both entity types; the FAQ order index is not unique, so a full replace saves in one step; Admin DTOs gained `isDeleted`/`deletedAt` but not a slug-history list; an unknown `categoryId` (not slug) still returns an empty page for existing mobile clients.

## Open items

None blocking. Measuring where the 2-hour product-detail import spends its time needs production logs after TS-003 is deployed (R-024); if the 1C SOAP call itself dominates, further speed-up needs a 1C-side change outside this scope.
