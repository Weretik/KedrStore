# Phase 03 — Application

- [x] T013 Update `src/Catalog/Catalog.Application/Integrations/OneC/Jobs/SyncOneCCategoryJob.cs` to resolve and apply approved metadata after final hierarchy construction, calculate levels, and retain the empty-response deletion guard. **Dependencies:** T009, T011.
- [x] T014 Preserve and extend the virtual Cosmos path in `SyncOneCCategoryJob.cs` so it applies approved metadata but does not call `GetCategories`; verify `src/Catalog/Catalog.Application/Integrations/OneC/Jobs/SyncOneCProductDetailsJob.cs` still assigns `CosmosCategoryId` directly. **Dependencies:** T010, T011.
- [x] T015 Update the no-tracking category read projection in `src/Catalog/Catalog.Application/Features/Category/GetList/` to build a deterministic `SortOrder`-first tree and include metadata only as required by the agreed consumer boundary. **Dependencies:** T003, T011.
- [x] T016 [P] Add import and query-handler tests under `tests/UnitTests/`, including hardware, doors, unknown fallback, and Cosmos fixtures. **Dependencies:** T013–T015.
- [x] T017 Verify cancellation tokens, repository usage, query non-tracking, and `Ardalis.Result` behavior in modified flows. **Dependencies:** T013–T016.

## Checkpoint

Imports are deterministic and side-effect safe; Cosmos does not call the unavailable operation; category reads do not write state.

## Verification (2026-07-29)

- `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~CategoryPresentationApplicationTests` — passed: 5/5. Covers configured hardware, doors, Cosmos, unknown-category fallback, and `SortOrder`-first category-tree output.
- `SyncOneCCategoryJob` checks the Cosmos root before any `GetCategoriesAsync` call and applies its resolved metadata. `SyncOneCProductDetailsJob` continues to map Cosmos products directly to `CosmosCategoryId`.
- The import path propagates its supplied cancellation token to all repository and 1C calls, preserves the non-empty-response deletion guard, and writes only through `ICatalogRepository<ProductCategory>`.
- `AllCategoriesSpec` remains `AsNoTracking`; `GetCategoriesQuryHandler` only reads through `ICatalogReadRepository<ProductCategory>` and returns `Result.NotFound()` or `Result.Success(...)` without write behavior.
