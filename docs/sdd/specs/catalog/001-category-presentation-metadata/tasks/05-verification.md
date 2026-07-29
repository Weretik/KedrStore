# Phase 05 — Verification

**Status:** completed and verified

- [x] T023 Run `dotnet restore KedrStore.sln` and record the result. **Dependencies:** T017, T022.
- [x] T024 Run `dotnet build KedrStore.sln --no-restore` and record the result. **Dependencies:** T023.
- [x] T025 Run `dotnet test KedrStore.sln --no-build` and record the result, distinguishing pre-existing failures from introduced failures. **Dependencies:** T024.
- [x] T026 Apply the migration to a disposable PostgreSQL database, run a non-overlapping full 1C import, and record migration/import logs without sensitive values. **Dependencies:** T025.
- [x] T027 Compare doors, hardware, and Cosmos category labels, order, and levels against `catalog-category-slugs.constants.ts.md` and `translate.md`; verify storefront constants remain until consumer cutover. **Dependencies:** T026.
- [x] T028 Complete `checklist/delivery-readiness.md`, update the specification and contract to match code, and prepare the delivery report. **Dependencies:** T027.

## T026-T028 completion record (2026-07-29)

- The migration was applied to a disposable PostgreSQL database and a non-overlapping full 1C import completed successfully. The initial endpoint-configuration blocker is resolved; no endpoint, credential, connection string, or other sensitive value is recorded here.
- Manual verification confirmed the persisted hardware, doors, and backend-owned Cosmos category metadata: Ukrainian/Russian short labels, sibling order, and derived levels match the frozen constants and translation snapshots. The frozen storefront constants remain in place until a separate consumer-cutover change.
- The Catalog Application consumer contract now records the additive `ShortNameUk`, `ShortNameRu`, `SortOrder`, and `Level` fields returned by `CategoryTreeDto`. No HTTP route or OpenAPI contract was introduced.

## Delivery report

- Delivered category presentation metadata through the domain model, EF Core migration, typed 1C configuration, import flow, Cosmos safety path, and deterministic application tree projection.
- Automated restore and build succeeded. The previously recorded full-solution test failures are outside this feature; targeted feature coverage and the manual migration/import verification passed.
- No rollout blocker remains. A future HTTP consumer must introduce an additive API contract before exposing the application read model.

## Verification record (2026-07-29)

- `dotnet restore KedrStore.sln` — succeeded. Existing NuGet vulnerability warnings remain for `Microsoft.OpenApi` and `Scriban`.
- `dotnet build KedrStore.sln --no-restore` — succeeded with the existing package vulnerability warnings and one nullable warning in `Identity.Infrastructure`.
- `dotnet test KedrStore.sln --no-build` — failed only outside this feature: `UnitTests.SlugTests.MapProduct_Should_Clean_Slugs` fails its existing slug expectation. All four integration tests completed successfully.

## T026 historical blocker (resolved)

The migration was applied successfully after PostgreSQL was started. A subsequent full import attempt used the supported `Host.Jobs --job=full` command, but stopped before category import at `SyncOneCPriceTypesJob`: `OneCSoap:Endpoint is not configured`. No category metadata import ran during that attempt.

T026–T028 require a usable non-production `OneCSoap:Endpoint` (and its credentials, if applicable) supplied through environment variables or user secrets. Once configured, run the full import again from the `Host.Jobs` directory and compare the persisted category metadata.
