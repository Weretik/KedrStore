# 010 — Retail prices from an Excel file

- **Module:** Catalog (prices of the [1C catalog import](../../../features/catalog/one-c-import.md), shown by the [public products](../../../features/catalog/public-products.md) and [public site models](../../../features/catalog/public-site-models.md))
- **Consumer:** React Admin «Каталог → Товари», button «Завантажити роздрібні ціни» (kedr-web spec `admin/catalog/retail-price-file`)
- **Status:** Accepted by the owner 2026-11-26 («делай» in the project thread); implemented 2026-11-26 (branch `claude/project-thread-najphy`); verified: unit 580/580, architecture 74/74, Catalog integration (in-memory and PostgreSQL) green
- **Source:** owner request of 2026-11-26: until the 1C programmer sends prices in their own currency, the retail price type «Розница халтмаг 30%» (id 11) is loaded from the 1C price list exported to Excel (`Цены_26.11.xlsx`)

## Goal and scope

The site shows one retail price in hryvnias, «Розница халтмаг 30%». The owner exports the 1C price list to Excel and uploads it in the Admin; only that price type changes, by product code. While the «do not update from 1C» box is on, the prices that came in the file stay as loaded: the 1C price sync does not change or remove them. Products that are not in the file keep getting their prices from 1C.

Given the owner uploads `Цены_26.11.xlsx` with the box on, when the 1C price sync runs, then the products from the file keep the file's retail price, the other products and all other price types follow 1C, and the site shows the file's price.

**Out of scope:** other price types from the file (the file holds them, but they keep coming from 1C); the old `.xls` format; editing one price by hand; a history of uploads beyond the staff activity journal (the Work middleware records each successful import and unlock under the tag `AdminRetailPrices`).

## Rules

- **RP-01.** The public price type is `Catalog:Pricing:RetailPriceTypeId`, now **11** (was 12 «РОЗНИЦА $»). Everything public reads it through the product list projection: the product list, price filter and sort, the product page, site models and their cards. The same setting is the retail fallback for customers without their own price type (Sales `DefaultPricePolicyProvider`, mobile catalog snapshot).
- **RP-02.** `POST /api/admin/retail-prices/import` takes a multipart `.xlsx` file up to 10 MB and `protectFromOneC` (default `true`). The file must have a header row with a cell «Код» and a cell equal to the name of the retail price type in the database («Розница халтмаг 30%»), compared trimmed and case-insensitively; columns may be in any order. Every row below with a code is a product: the code is the product id. A row without a code (title rows, groups) is ignored.
- **RP-03.** Only the retail price type changes. A code that is not a product in the catalog goes to `notFoundCodes`; a row whose price is empty, not a number, or not above zero goes to `withoutPriceCodes`; both are reported, never stored. A price is UAH rounded to two decimals; a value the price rules refuse (above 5 000 000) also goes to `withoutPriceCodes`. When a code repeats, the last row wins. Products absent from the file keep their prices.
- **RP-04.** With `protectFromOneC = true` every retail price from the file is locked from 1C (`LockedFromOneCAt` = upload time); with `false` the prices from the file are stored and their lock is removed, so the next 1C sync may change them. Prices of products not in the file keep their lock state.
- **RP-05.** The 1C price sync neither changes nor removes a locked price; it counts it as kept. Unlocked prices, other price types and other products sync as before.
- **RP-06.** `POST /api/admin/retail-prices/unlock` removes the lock from every retail price («галочку зняли»); the prices stay until the next 1C sync brings 1C's values. A repeat changes nothing.
- **RP-07.** `GET /api/admin/retail-prices` returns the retail price type (id, name), how many of its prices are locked from 1C, and the last lock time.
- **RP-08.** The import saves once and then rebuilds the product list projection in the same request, so the Admin and the site lists show the new prices right away (the site's own cache still applies). The answer reports `changed`, `unchanged`, `notFoundCodes`, `withoutPriceCodes` (each list at most 50 codes, with the total counts) and the status of RP-07.
- **RP-09.** The three endpoints require policy `CanManageProducts`. Import and unlock use the `HeavyOperation` rate limit (an import that rewrites prices and rebuilds the projection); the status read stays under the global limit.

## Scenarios

| ID | Scenario | Evidence |
| --- | --- | --- |
| **SC-RP01** | Domain: a file price is stored in UAH and locked or unlocked by the flag; a 1C update of a locked price is refused with `Catalog.ProductPrice.LockedFromOneC`; unlock clears the lock; a negative or too large amount is refused. | `ProductPriceLockTests` (unit) |
| **SC-RP02** | Sheet reader: finds «Код» and «Розница халтмаг 30%» in any column after title rows, reads codes written as text or number, skips group rows, reports a missing column and a file that is not `.xlsx`. | `RetailPriceSheetReaderTests` (integration, no Docker) |
| **SC-RP03** | Import: product 1 gets a new locked price, product 2 changes, product 3 unchanged, code 999 not found, a row without price reported, product 4 not in the file untouched, wholesale price untouched; one save; with `protectFromOneC = false` the lock of product 2 is removed. | `ImportRetailPricesTests` (integration, in-memory) |
| **SC-RP04** | 1C sync: a locked retail price keeps its amount and is not removed when 1C stops sending it; the unlocked one and the wholesale price follow 1C; after unlock the next sync changes it. | `PriceImportTests` (integration, in-memory) |
| **SC-RP05** | Validators and row rules: no file, an empty file, a file over 10 MB answer 400 with Ukrainian messages; codes and prices of the rows as RP-02 and RP-03 say. | `ImportRetailPricesValidatorTests`, `RetailPriceFileRowsTests` (unit) |
| **SC-RP06** | HTTP: anonymous 401, customer 403, manager 200; the upload is locked by default, `protectFromOneC=false` leaves it to 1C, the status shows type 11 and its name, unlock answers 0 locked, a text file answers 400 with the reason. | `RetailPricesApiTests` (integration, in-memory) |
| **SC-RP07** | Access and limits: the import uses `HeavyOperation`; operation ids and summaries exist; both commands drop the site models state; the commands without input need no validator. | `RateLimitCoverageTests`, `OpenApiDocumentRulesTests`, `ReadCacheTests`, `ValidatorCoverageTests` (architecture) |

## Design

**Aggregate.** `ProductPrice` (aggregate root, `Catalog.Domain/Products/Prices/`) gets `LockedFromOneCAt` (`DateTimeOffset?`) and:
- `UpdateFromOneC(Money price)` → `DomainResult`: refuses a locked price (`ProductPriceErrors.LockedFromOneC`), otherwise the old checks; the 1C job calls it instead of `Update`;
- `CanBeRemovedByOneC` (a locked price cannot);
- `SetFromFile(Money price, bool lockFromOneC, DateTimeOffset now)` → `DomainResult`; factory `CreateFromFile(...)` → `DomainResult<ProductPrice>` for a product without a retail price yet (its `ProductTypeIdOneC` is the product's);
- `UnlockFromOneC()`.

**Write side.**
- `ImportRetailPrices` command (`Features/RetailPrices/Commands/ImportRetailPrices/`): validator (file present, 1 byte … 10 MB); handler reads the retail price type name, asks the `IRetailPriceSheetReader` port for rows, loads the retail prices of the found products tracked and the product ids in two queries, applies `SetFromFile` / `CreateFromFile`, saves once, then rebuilds the projection. Repeat: target values, so the same file again gives the same state and reports the rows as `unchanged` (idempotency rules: an import is an upsert, no request key). Logged: `Retail prices imported: changed {Changed}, unchanged {Unchanged}, not found {NotFound}, without price {WithoutPrice}, locked {Locked}`.
- `UnlockRetailPrices` command (`Commands/UnlockRetailPrices/`): loads the locked retail prices tracked and calls `UnlockFromOneC`, one save; state check makes a repeat a no-op.
- `SyncOneCPricesJob`: updates through `UpdateFromOneC` (a locked price is skipped and counted `kept`) and removes only prices with `CanBeRemovedByOneC`.

**Read side.** `GetRetailPriceStatus` query (through `Shared/RetailPriceStatusReader`, which the commands reuse for their answers) reads `ProductPrices` with `AsNoTracking` (`Count`, `Max(LockedFromOneCAt)`) and `PriceTypes`; it never loads aggregates. The import answer reuses it.

**Excel.** Port `Catalog.Application/Abstractions/Files/IRetailPriceSheetReader` returns `Result<RetailPriceSheet>` (rows of code text and price cell); `Catalog.Infrastructure/Integrations/Excel/ClosedXmlRetailPriceSheetReader` reads the first worksheet with ClosedXML (already a central package). No new library, external system, or background mechanism, so no ADR.

**Why not an outbox or a job.** Nothing leaves the system and no other module has to learn about the change: the admin's request writes Catalog's own table. 1 600 rows are two queries, one save, and one projection rebuild, a few seconds, so the request does it directly.

**Database.** Column `catalog."ProductPrices"."LockedFromOneCAt"` (`timestamptz`, null) — additive migration `ProductPriceLockFromOneC`, the previous release ignores it. Concurrency: `xmin` row version on `ProductPrice`, so a 1C sync that read a price before an upload locked it fails its save instead of overwriting it (database rules 4); an import that races a sync answers 409 «Ціни саме оновлюються з 1С. Спробуйте ще раз.». The status counts use the existing (`ProductId`, `PriceTypeId`) index scan on about 1 600 retail rows; no new index.

**Contract.** Tag `AdminRetailPrices`:

| Method and route | Operation id | Body | Answer |
| --- | --- | --- | --- |
| `GET /api/admin/retail-prices` | `getAdminRetailPriceStatus` | — | `RetailPriceStatusDto` |
| `POST /api/admin/retail-prices/import` | `importAdminRetailPrices` | multipart `file`, `protectFromOneC` | `RetailPriceImportResultDto`; 400 validation or a sheet problem; 409 race with 1C; 413 |
| `POST /api/admin/retail-prices/unlock` | `unlockAdminRetailPrices` | — | `RetailPriceStatusDto` |

**Architecture questions.** Volume: about 1 600 rows, one upload a day at most; P95 of the import under 10 s (system default otherwise, [quality targets](../../../architecture/overview/quality-targets.md)). No new library, database, or external service. Money: prices only, no stock or limits; the amount comes from the owner's file, the user id from the session. Personal data: none. Rate limits: RP-09. Action journal: the Work activity middleware records the import and the unlock (who and when). Permission: `CanManageProducts` (Manager, Admin), as for other product edits.

**Rollout and rollback.** Deploy order: migration job, then API. `RetailPriceTypeId` 11 is in `appsettings.json` of `Host.Api` and `Host.Jobs` (no environment override exists); after the deploy the projection holds type 12 until the next rebuild, so the owner uploads the file or the next 1C price sync rebuilds it. Rollback: set the setting back to 12; the column stays harmless.

## File-role map

| Role | File |
| --- | --- |
| Aggregate and errors | `Catalog.Domain/Products/Prices/ProductPrice.cs`, `ProductPriceErrors.cs` |
| Mapping, migration | `Catalog.Infrastructure/Persistence/Products/Prices/ProductPriceConfiguration.cs`, `Migrations/*_ProductPriceLockFromOneC.cs` |
| Import command (RP-02…RP-04, RP-08) | `Catalog.Application/Features/RetailPrices/Commands/ImportRetailPrices/` (command, validator, handler, `RetailPriceFileRows` for the row rules) |
| Unlock command (RP-06) | `Catalog.Application/Features/RetailPrices/Commands/UnlockRetailPrices/` |
| Status query (RP-07) and shared reads | `Catalog.Application/Features/RetailPrices/Queries/GetRetailPriceStatus/`, `Features/RetailPrices/Shared/` (`RetailPriceStatusReader`, `RetailPriceSave`: one save, 409 on a 1C race) |
| Sheet port and reader | `Catalog.Application/Abstractions/Files/IRetailPriceSheetReader.cs`, `Catalog.Infrastructure/Integrations/Excel/ClosedXmlRetailPriceSheetReader.cs`, `RetailPriceSheetHeader.cs` |
| 1C sync (RP-05) | `Catalog.Application/Features/OneCSync/Jobs/SyncOneCPricesJob.cs` |
| Endpoints (RP-09) | `Catalog.Api/Endpoints/RetailPrices/` (`GetAdminRetailPriceStatus/`, `ImportAdminRetailPrices/`, `UnlockAdminRetailPrices/`, `RetailPricesEndpoints.cs`) |
| Contract DTOs | `Catalog.Contracts/RetailPrices/RetailPriceStatusDto.cs`, `RetailPriceImportResultDto.cs`, [OpenAPI](../../../contracts/openapi.json) |
| Setting (RP-01) | `Host.Api/appsettings.json`, `Host.Jobs/appsettings.json` |

## Tasks

- **TS-01.** Tests first: SC-RP01…SC-RP06 red.
- **TS-02.** Aggregate, mapping, migration (SC-RP01).
- **TS-03.** Sheet reader (SC-RP02).
- **TS-04.** Import, unlock, status; 1C sync lock (SC-RP03, SC-RP04, SC-RP05).
- **TS-05.** Endpoints, OpenAPI, setting 11 (SC-RP06).
- **TS-06.** Feature card, feature and module maps, final audit with `backend-code-audit`, findings fixed in the same PR.

## Final audit (2026-11-26, `backend-code-audit`)

Quality targets read first: writes P95 2 s and P99 5 s (confirmed/proposed, met by the API overall). The import is a rare staff upload (about once a day) of about 1 600 rows.

| ID | Severity | Status | Finding | Change and evidence |
| --- | --- | --- | --- | --- |
| AF-01 | High | Fixed | The import and unlock commands did not drop the cached site models state, which shows retail prices, so model pages could show old prices for the cache lifetime. | Both commands implement `IInvalidatesReadCache` with `SiteModelStateReader.Tag`; `ReadCacheTests` green. |
| AF-02 | Medium | Fixed | The validator's Ukrainian message applied only to the upper size limit; an empty file got the English default text. | `Must(length is > 0 and <= Max)` with one message; `ImportRetailPricesValidatorTests` green. The same pattern in `UploadAttributeSwatchValidator` is pre-existing (AF-06). |
| AF-03 | Low | Fixed | The spec said Catalog edits are not journaled; the Work middleware journals every `Admin*` change. | Scope and architecture answers corrected. |
| AF-04 | Low | Accepted | The import commits the prices, then rebuilds the list projection in its own transaction: if the rebuild fails, prices are saved and the lists show the old ones until the next rebuild. | Re-uploading the same file is safe (upsert) and rebuilds; the 1C price sync rebuilds too. |
| AF-05 | Low | Open (measure) | The import includes a full projection rebuild, so its answer may exceed the 2 s write P95 target. | One request a day does not move the P95; check its latency in Cloud Run logs after the first uploads. |
| AF-06 | Low | Pre-existing | `UploadAttributeSwatchValidator`: the Ukrainian message covers only the upper limit (same as AF-02). | Outside this scope; fix with the next swatch change. |

Checked without findings: module isolation (Catalog only, no new Contracts consumer), folder axiom and wrapper rules (`FolderStructureTests`), use case independence (`Shared/` for the status reader and the save), aggregate rules (changes through `ProductPrice`, factories return `DomainResult`, `UpdateFromOneC` refuses a locked price), one `SaveChanges` per command, no query in a loop (two reads, one save), race protection (`xmin`, 409), idempotency (upsert by code, state check for unlock, row added to the idempotency audit), validators and Ukrainian messages, rate limits (`HeavyOperation`, `RateLimitCoverageTests`), OpenAPI ids and summaries (`OpenApiDocumentRulesTests`), no ADR needed (no new library family, system, or mechanism: ClosedXML is already a central package).
