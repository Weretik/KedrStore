# Refresh stock projection

**Module:** catalog\
**Status:** implemented\
**Owners:** backend team\
**Created:** 2026-07-27\
**Related:** Catalog import tasks from 1C

## Goal

As a catalog user, I want the targeted stock import to refresh the product-list read model so that the filter and `InStock` values remain current.

## Boundaries

- In scope: rebuild `ProductListProjections` after a successful targeted `stocks` import.
- Out of scope: incremental or partial projection refresh.
- Assumption: a non-empty 1C stock response is authoritative for the selected root.
- Dependencies: `IProductListProjectionRebuilder` and the existing catalog job host.

## Scenarios

1. If the targeted stock import updates product quantities, rebuild the product-list projection after it completes.
2. During a full import, stock stages do not independently rebuild the projection; the full job rebuilds it once at the end.

## Contract and compatibility

- No API contract document is needed because the API contract does not change.
- Existing consumers and compatibility: `stocks` now refreshes `InStock` automatically; CLI arguments are unchanged.
- Authorization and idempotency: not applicable to the CLI job.

## Implementation stages

| Stage | Status | Required? | Result |
| --- | --- | --- | --- |
| Application | implemented | yes | The targeted stock import job refreshes the read model; the full job suppresses intermediate refreshes. |
| Documentation | implemented | yes | Import flow description and runbook reflect the new behavior. |

## Files

- Change: `SyncOneCStocksJob`, `SyncOneCFullJob`, Catalog import documentation, and runbook.
- Do not change: database schema, public HTTP contracts, projection schema.

## Acceptance criteria

- [x] Targeted `stocks` rebuilds `ProductListProjections` after saving stock values.
- [x] `full` still performs one final projection rebuild.
- [x] CLI runbook documents this behavior.

## Verification

- `dotnet restore KedrStore.sln`: succeeded, with existing package vulnerability warnings.
- `dotnet build KedrStore.sln --no-restore`: succeeded, 0 errors and 28 existing warnings.
- `dotnet test KedrStore.sln --no-build --no-restore`: did not complete successfully: `SlugTests.MapProduct_Should_Clean_Slugs` fails independently of this change; integration tests cannot connect to PostgreSQL at `127.0.0.1:5432`.

## Risks and open questions

- [ ] Projection refresh still replaces the whole table; a separately designed incremental update mechanism may be useful for large catalogs.

## Change history

- 2026-07-27 — Implemented automatic product-list projection refresh after targeted stock synchronization.
