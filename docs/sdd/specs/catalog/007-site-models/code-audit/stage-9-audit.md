# Feature audit: spec catalog/007 stage 9 (a model's own category)

- **Scope:** `ProductGroup.ChangeCategory`, `Commands/ChangeSiteModelCategory/`, `Commands/AcceptSiteModels/`, `Commands/CreateSiteModel/`, `Commands/AddSiteModelProducts/`, `Shared/State/{SiteModelChanges,SiteModelSnapshot,SiteModelState,SiteModelFacts}.cs`, `Shared/SiteModelMapper.cs`, `Queries/GetSiteModelCategories/`, `Shared/Public/PublicModelCatalogReader.cs`, `Catalog.Contracts/SiteModels/`, endpoint `ChangeAdminSiteModelCategory`, `openapi.json`, tests of stage 9.
- **Date:** 2026-11-26, after implementation, by skill `backend-code-audit`.
- **Sources:** [module rules](../../../../standards/module-rules.md), [database rules](../../../../standards/database-rules.md), [idempotency rules](../../../../standards/idempotency-rules.md), [rate limit rules](../../../../standards/rate-limit-rules.md), [quality targets](../../../../architecture/overview/quality-targets.md), the new-feature and change-gates review lists.
- **Method:** static review; `dotnet build` of `Host.Api` (regenerated `openapi.json` equals the committed file); unit tests 560, architecture tests 74, integration tests `IntegrationTests.Catalog` 98 passed and 14 skipped (Postgres-only, run in CI); CI of PR #273 green.

## Step 0: quality targets

Reads P95 ≤ 1 s (current 328 ms), writes P95 ≤ 2 s (current 1 159 ms), catalogue about 2 330 products. The admin lists read the cached site models snapshot (1 minute, ADR-0016); the public catalogue reader keeps its 2-minute cache and loads the same exported products as before (the category filter moved after the load). The category change is one admin write in one transaction: load the model, the final-category check (two `Any`), at most the accept steps of «Прийняти», one `SaveChanges`. No speed risk against the targets.

## Checks

| Check | Result |
| --- | --- |
| Module and layers | Everything stays in Catalog, which owns models, products and categories; the rule (category change) is on the aggregate, the handler only orders steps; the endpoint maps the request to the command and the result through `ToHttpResult`. |
| Use case shape | `Commands/ChangeSiteModelCategory/` holds command, handler, validator; the validator checks input only (ids > 0); business checks (final category, unknown model) return `Result` (400 `Catalog.SiteModel.CategoryNotFinal`, 404). |
| DDD | `ChangeCategory` returns whether it changed, private setter, `UpdatedAt`; accepting reuses `ProductGroup.Accept` through `SiteModelChanges.AcceptAsync`, shared with «Прийняти» (no duplicate logic). |
| Database | Reads without tracking in the readers; no query or `SaveChanges` in a loop added (pins per accepted model as before inside `AcceptAsync`, one model here); the command runs in `ICatalogTransaction`, so a stop half-way leaves nothing (accept, pins, build and category commit together). No migration: the column `CategoryId` already exists. |
| Races | Two category changes: last write wins on a set value; accept is a no-op when already accepted. |
| Idempotency | A set value: a repeat gives the same state; no key needed (recorded in the stage). |
| Rate limit | Signed-in Manager or Admin call: the global soft limit (recorded in the stage); `RateLimitCoverageTests` pass. |
| Contract | `WithName("changeAdminSiteModelCategory")`, summary, `Produces<SiteModelDto>`, 400/403/404 problems; `ApiContractTests` assert the path and operation. |
| Folder axiom | Architecture tests (`FolderStructureTests`) pass; the new folder has 3 files. |
| ADR / C4 | No new module, host, external system or way to run work: nothing to record. |
| Docs | Feature cards `attributes.md` and `public-site-models.md` updated; doc link and module map checks pass. |

## Findings

| ID | Severity | Evidence | Rule and impact | Status |
| --- | --- | --- | --- | --- |
| AF-001 | low | `SiteModelChanges.CheckProductsAsync` answered the product errors in English («Products not found», «Products already in another model») | Messages for users are in Ukrainian; the admin shows the server text | fixed: Ukrainian texts; build and Catalog integration tests pass |
| AF-002 | low | `GetSiteShowcaseHandler` keeps filtering a product in no model by its own category | Matches R-24 (a product in no model is placed by its own category); listed in the stage file tree by mistake | fixed: file tree corrected, no code change needed |

Open findings in scope: none.
