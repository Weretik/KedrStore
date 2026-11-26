# 012 — Categories made by hand

**Module:** Catalog. **Status:** accepted 2026-11-26 (owner: «да» to the plan in the thread «Улучшить UX страницы «Вітрина сайту»»). **Date:** 2026-11-26.

The owner asked for a full manual category: create it in the admin «Категорії», choose where in the tree it sits («в какой уровень вложить»), and put site models (with their products) into it. Then (same day) for managing every category: move any category, 1C ones too, to any level and change the order, with the rule «what we change on the site wins over what comes from 1C»; and no «як у 1С» marks in the admin. Until now every category came from 1C or from the fixed groups in configuration (`HardwareManualCategoryGroups`, `DoorsManualCategoryGroups`).

## Quality targets

Admin-only, rare writes (a few categories a month). The category tree read keeps its current target in [quality targets](../../../architecture/overview/quality-targets.md); a manual category adds one row to it. No new hot path.

## Goal and boundaries

A person who may manage the site catalog creates a site category by hand, moves any category under any other or to the top, changes the order of neighbours, renames through the site names of spec [011](../011-site-search-settings/README.md), deletes a manual category while empty, and puts site models into a category. The 1C sync keeps all of it.

**Out of scope:** deleting 1C categories (the next sync would bring them back; hiding from the site is the way); putting a single product without a model into a category (it gets a model through «Нова модель»); writing anything to 1C; the configured manual groups (they stay as they are); showing 1C places or names in the admin.

## Rules

- **MC-01.** A manual category has `IsManual = true`, an id from its own database sequence (from 800 000 000, outside 1C codes and the configured groups), the parent's `ProductTypeIdOneC` (`manual` at the top), a slug from its Ukrainian name, a path under its parent, and the parent's «show on site» and «closed from search» unless the request says otherwise.
- **MC-02.** Create takes `nameUk`, `nameRu` (both required, at most 100 characters), an optional `parentId` (an active category), and an optional `isShownOnSite`. The names are stored as the 1C name and the short names (UK, RU) and as the site names of spec 011, so every reader shows them. A category with the same Ukrainian name under the same parent answers 409 `CategoryNameTaken`.
- **MC-03.** Any category, 1C or manual, moves under any active category without site models or to the top, never under itself or its own subcategory (400). Its path and the paths of its subcategories change in the same save. A parent with site models is refused for a create and a move (400), since a model sits only in a final category (spec 007 M-03).
- **MC-04.** A manual category is deleted (soft, as the sync does) only when it has no active subcategories, site models, or products; otherwise 409. A 1C category answers 400 `Catalog.Category.NotManual`. A deleted manual category is not restored; a new one is created instead.
- **MC-05.** The 1C category sync never deletes, renames, or moves a manual category. When a 1C parent of a manual category moves, the manual category and its manual subcategories follow it (paths rebuilt in the same save). When the 1C parent is deleted, the manual category stays and is hidden with it from the site; the admin shows it under the deleted parent.
- **MC-06.** A final manual category (no subcategories) is a model category like any other final category (spec 007 M-03, R-22): «Категорія на сайті» offers it and `PUT api/admin/site-models/{id}/category` accepts it.
- **MC-07.** «Додати моделі» puts several models into one final category in one save: each model changes its category by R-22 (a proposed model is accepted in the same step). An unknown model answers 404, a non-final category 400 `CategoryNotFinal`.
- **MC-08.** Admin category reads carry `isManual`, so the admin marks «Створена вручну» and shows «Видалити» only for manual categories.
- **MC-09.** A place set by hand wins over 1C: the sync remembers the 1C parent (`OneCParentId`) but keeps `ParentId` of a category moved by hand (`IsPlacedByHand`); moving it back under its 1C parent clears the mark. After every sync all paths are rebuilt from the parents, so a new 1C subcategory of a moved category sits under it where it is now.
- **MC-10.** «Вище» / «Нижче» swaps a category with its neighbour in the shown order (sort order, then id) and numbers the neighbours anew by hand (`IsOrderedByHand`); the sync keeps that order.

**Rate limit.** All new endpoints are cheap signed-in Manager or Admin calls: the global soft limit is enough (rate limit rules). **Idempotency.** Create is not money, stock, an order, a numbered document, or a message: a repeat is stopped by the admin's pending lock and, if it gets through, by MC-02's 409 on the same name under the same parent; no key. Move, delete, and add models set a state: a repeat gives the same state.


## API

| Operation | Method and path | Body | Answer |
| --- | --- | --- | --- |
| `createAdminCategory` | `POST api/categories` | `nameUk`, `nameRu`, `parentId?`, `isShownOnSite?` | 201 `AdminCategoryResponse` |
| `moveAdminCategory` | `PUT api/categories/{id}/parent` | `parentId?` | `AdminCategoryResponse` |
| `reorderAdminCategory` | `PUT api/categories/{id}/order` | `direction`: `up` or `down` | `AdminCategoryResponse` |
| `deleteAdminCategory` | `DELETE api/categories/{id}` | — | 204 |
| `addAdminCategorySiteModels` | `POST api/categories/{id}/site-models` | `modelIds` (1–200) | 204 |

All use the policy `CanManageSiteCatalog`, as «show on site».

## Data

`catalog.ProductCategories`: new columns `IsManual`, `IsPlacedByHand`, `IsOrderedByHand` (bool, default false) and `OneCParentId` (int, filled from `ParentId` by the migration); new sequence `catalog."ManualCategoryIds"` starting at 800000000. Migration `ManualCategories` (additive; the previous release ignores the columns).

## Design

- Domain: `ProductCategory.Placement.cs` (`CreateManual`, `MoveUnder`, `FollowParent`, `SetOrder`, `EnsureManual`), `UpdateFromOneC` keeps a place by hand and ignores a moved ancestor, `UpdatePresentationMetadata` keeps an order by hand; `CategoryTreePlacement.Rebuild` sets every path from its parent (loops and missing parents go to the top).
- Application `Features/CategoryPlacement/`: `CreateManualCategory`, `MoveCategory`, `ReorderCategory`, `DeleteManualCategory`, `AddCategorySiteModels` (reuses `SiteModelChanges.AcceptAsync` and `ProductGroup.ChangeCategory`), and `Shared/CategoryTreeEditor` (whole tree tracked, parent checks, rebuild and one save, answer). Each command drops the category tree and site models caches.
- Sync: `MarkMissingDeleted` skips manual categories; the presentation level comes from where the category sits now; after applying 1C rows the whole tree is rebuilt in the same save.
- `IManualCategoryIds` → `ManualCategoryIds` (`nextval`).

## Scenarios

- **SC-MC01 (MC-01, MC-02, MC-08).** «Накладні ручки» is created under «Ручки»: it has a new id from 800 000 000, the parent's visibility and search closing, a slug, `isManual`; the tree shows it under «Ручки»; the same name again answers 409.
- **SC-MC02 (MC-03, MC-09).** 1C «Замки» moves under «Двері» with its subcategory and back; a move under its own subcategory answers 400; a parent with models is refused.
- **SC-MC06 (MC-10).** «Нижче» on the first top category swaps it with the second; a wrong direction answers 400; a sync keeps the order (unit).
- **SC-MC03 (MC-06, MC-07).** Two models are added to it: one proposed (accepted with it), one accepted; the public category lists both with their products; «Категорія на сайті» offers it.
- **SC-MC04 (MC-04).** Delete answers 409 while it has a model or a subcategory, and 204 when empty; it moves to «Видалені».
- **SC-MC05 (MC-05, MC-09).** After «Замки» is moved under «Ручки» and a manual category is put under «Врізні», a sync that renames «Замки» and adds «Навісні» under it keeps «Замки» under «Ручки» with the new name, puts «Навісні» there too, and keeps the manual category under «Врізні».

## File-role map

```text
src/Modules/Catalog/
├── Catalog.Domain/Categories/ProductCategory.cs, ProductCategory.Placement.cs, CategoryErrors.cs, Tree/CategoryTreePlacement.cs
├── Catalog.Contracts/Category/CategoryPlacementRequests.cs, CategoryDtos.cs   # AdminCategoryResponse.IsManual
├── Catalog.Application/Abstractions/Persistence/IManualCategoryIds.cs
├── Catalog.Application/Features/CategoryPlacement/
│   ├── Commands/CreateManualCategory/, MoveCategory/, ReorderCategory/, DeleteManualCategory/, AddCategorySiteModels/
│   └── Shared/CategoryTreeEditor.cs
├── Catalog.Application/Features/OneCSync/Jobs/SyncOneCCategoryJob.cs          # MC-05, MC-09
├── Catalog.Infrastructure/Persistence/Categories/ProductCategoryConfiguration.cs, ManualCategoryIds.cs
├── Catalog.Infrastructure/Persistence/CatalogDbContext.cs                      # sequence
├── Catalog.Infrastructure/Persistence/Migrations/20261126131415_ManualCategories.cs
└── Catalog.Api/Endpoints/CategoryPlacement/                                    # five endpoints and their map
tests/
├── UnitTests/Catalog/Domain/Categories/ManualCategoryTests.cs, CategoryPlacementTests.cs
├── IntegrationTests/Catalog/Categories/CategoryPlacementApiTests.cs           # SC-MC01, SC-MC02, SC-MC04, SC-MC06
├── IntegrationTests/Catalog/Import/CategoryImportPlacementTests.cs            # SC-MC05
└── IntegrationTests/Catalog/SiteModels/SiteModelCategoryTests.cs              # SC-MC03
```

Frontend: kedr-web `docs/specs/admin/categories/002-manual-categories`.

## Final audit (2026-11-26)

Checked against the quality targets, module rules, database, idempotency, and rate limit rules.

| ID | Severity | Status | Finding | Change and evidence |
| --- | --- | --- | --- | --- |
| AF-012-01 | Medium | fixed | `Catalog.Domain/Categories` held 7 code files (folder axiom). | Tree-wide rules moved to `Categories/Tree/` (`CategorySearchClosing`, `CategorySiteVisibility`, `CategoryTreePlacement`); `FolderStructureTests` pass. |
| AF-012-02 | High | fixed | The product import matched a product's 1C folder by name among our categories, so a manual category or a configured group with a 1C folder's name made it ambiguous, and the product kept an old category (owner's report: a model «на планці» showed «На розетці»). | The import now matches against the folders 1C lists (path, then name) and skips manual categories (spec 005 R-023, SC-014a); `ImportSyncTests`, `ProductImportSoftDeleteTests` pass. |
| AF-012-03 | Low | accepted | Two creates with the same name at the same moment can both pass the name check (no unique index on name and parent). | Admin-only and rare; the admin's pending lock stops a double click. Revisit if it is seen. |

Commits per use case: each command makes one `SaveChanges` (add models: one transaction with the model acceptance); a stop half-way leaves nothing. PostgreSQL tests (migration, sequence) need Docker and run in CI.
