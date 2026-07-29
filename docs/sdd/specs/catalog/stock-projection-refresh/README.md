# Stock projection refresh

**Module:** catalog  
**Status:** implemented  
**Owner:** backend team  
**Created:** 2026-07-27  
**Related:** Catalog 1C import jobs

## Goal

As a catalog user, I want a targeted stock import to refresh the product-list read model so that the `InStock` filter and value remain current.

## Scope

- In scope: rebuild `ProductListProjections` after a successful targeted `stocks` import.
- Out of scope: incremental/partial projection updates.
- Assumption: a non-empty 1C stock response is authoritative for the selected root.
- Dependencies: `IProductListProjectionRebuilder` and the existing catalog jobs host.

## Scenarios

1. Given a targeted stock import updates product stock, when it finishes, then the product-list projection is rebuilt.
2. Given a full import, when its stock phases run, then they do not rebuild independently and the full job rebuilds once at the end.

## Contract and compatibility

- API contract document: not needed because no API contract changes.
- Existing consumers and compatibility: `stocks` now refreshes `InStock` automatically; CLI arguments stay unchanged.
- Authorization and idempotency: not applicable to the CLI job.

## Implementation phases

| Phase | Status | Required? | Outcome |
| --- | --- | --- | --- |
| Application | implemented | yes | Targeted stock job triggers the read-model refresh; full job suppresses intermediate refreshes. |
| Documentation | implemented | yes | Import-flow and runbook reflect the new behavior. |

## Files

- Modify: `SyncOneCStocksJob`, `SyncOneCFullJob`, catalog import documentation and runbook.
- Do not change: database schema, public HTTP contracts, projection schema.

## Acceptance criteria

- [x] Targeted `stocks` rebuilds `ProductListProjections` after stock persistence.
- [x] `full` still performs a single final projection rebuild.
- [x] CLI runbook documents the behavior.

## Verification

- `dotnet restore KedrStore.sln`: passed, with pre-existing package vulnerability warnings.
- `dotnet build KedrStore.sln --no-restore`: passed, 0 errors and 28 existing warnings.
- `dotnet test KedrStore.sln --no-build --no-restore`: could not pass: `SlugTests.MapProduct_Should_Clean_Slugs` fails independently of this change; integration tests cannot connect to PostgreSQL at `127.0.0.1:5432`.

## Risks and open questions

- [ ] Projection rebuilding remains full-table replacement; large catalogs may benefit from a separately designed incremental projection mechanism.

## Change log

- 2026-07-27 — Implemented automatic product-list projection refresh after targeted stock sync.
