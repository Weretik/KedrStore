# Category presentation metadata and Cosmos import safety

**Module:** catalog  
**Status:** draft  
**Owner:** backend team  
**Created:** 2026-07-27  
**Related:** [frozen storefront constants snapshot](catalog-category-slugs.constants.ts.md); [frozen UA/RU translations snapshot](translate.md); 1C catalog import

## Goal

As a catalog administrator, I want the original 1C category name, localized short display labels, ordering and nesting metadata to be imported and configured in the backend so that the storefront no longer needs a manually maintained category-slug constants file.

## Scope

- In scope: original full name from 1C in the existing `Name` field; configured Ukrainian and Russian short display names; `SortOrder`; `Level`; migration; API/read-model exposure where required; and a safe Cosmos-category fallback.
- Out of scope: modifying the storefront repository in this task, changing product/category identifiers, or changing generated 1C SOAP code.
- Assumptions: the included constants snapshot is the authoritative baseline for current UA short labels, category slugs and display ordering; the [translations snapshot](translate.md) is authoritative for current RU short labels.
- Dependencies: the frozen [constants snapshot](catalog-category-slugs.constants.ts.md), [translations snapshot](translate.md), 1C category payloads for doors and hardware, and configured metadata for the backend-owned virtual Cosmos category.

## Scenarios

1. Given a 1C category is imported, when it is stored, then its original 1C name is retained in `Name`, independently of its short display labels.
2. Given category presentation configuration, when a category is imported or queried, then it has configured UA/RU short labels, `SortOrder` and `Level`.
3. Given Doors, Hardware or Cosmos root categories, when a category tree is returned, then root categories have level 0 and their direct presentation groups have level 1.
4. Given Cosmos has no categories SOAP operation, when its product/details job runs, then every returned product is assigned to the one backend-owned Cosmos category and no category SOAP call is required.

## Contract and compatibility

- API contract document: required in phase 05 if existing category DTOs are extended or a new category-tree response is added.
- Existing consumers and compatibility: keep current category IDs, slugs and routes stable; expose new fields additively.
- Authorization and idempotency: read contracts remain subject to current authorization; 1C CLI jobs have no HTTP idempotency contract.

## Implementation phases

| Phase | Status | Required? | Outcome |
| --- | --- | --- | --- |
| [01 Configuration discovery](phases/01-configuration-discovery.md) | ready | yes | Map the frozen frontend constants snapshot to stable backend configuration entries. |
| [02 Domain model](phases/02-domain-model.md) | draft | yes | Add category naming and presentation metadata with explicit invariants. |
| [03 Persistence](phases/03-persistence-migration.md) | draft | yes | Persist fields, add migration, backfill safely and index ordering if needed. |
| [04 Import and Cosmos safety](phases/04-import-and-cosmos-safety.md) | draft | yes | Apply configuration during category import and make Cosmos product import safe without 1C categories. |
| [05 Query contract and handoff](phases/05-query-contract-and-handoff.md) | draft | conditional | Additive API/query exposure and frontend migration plan, only if current contracts lack required category metadata. |
| [06 Verification and rollout](phases/06-verification-and-rollout.md) | draft | yes | Tests, manual 1C checks and safe rollout order. |

## Files

- Create: this SDD and its phase documents.
- Planned modification: Catalog domain, EF configuration/migration, category import/mapping, typed 1C options, relevant category queries/contracts, and host configuration.
- Do not change in early phases: public routes, generated SOAP client, existing product/category IDs and slugs.

## Acceptance criteria

- [ ] Original 1C name, `ShortNameUk` and `ShortNameRu` are independently stored.
- [ ] `SortOrder` and `Level` are persisted and populated deterministically from configuration/tree rules.
- [ ] The same configuration replaces the current frontend ordering/label mapping without a duplicated manual list.
- [ ] Cosmos import assigns every Cosmos product to the backend-owned Cosmos category without calling or relying on `GetCategories`.
- [ ] Existing category consumers remain compatible during rollout.

## Verification

- Unit: mapping, level calculation, metadata fallback and Cosmos category resolution.
- Integration: EF migration and import persistence against PostgreSQL.
- API: additive category response contract, when phase 05 applies.
- Manual: compare the resulting tree and order to the current storefront constants.
- Commands: execute restore, build and test after each implementation phase that changes code.

## Risks and open questions

- [ ] The frontend snapshots cover only configured navigation categories; phase 01 must define the fallback short names and ordering for new/unconfigured categories.
- [ ] Cosmos has no `GetCategories` operation by design. Its one virtual category requires a stable backend ID and configured UA/RU full/short names and sort order.
- [ ] Cosmos virtual-category import is implemented separately; phase 04 must preserve that behavior while adding presentation metadata.

## Change log

- 2026-07-27 — Initial phased specification created; no implementation changes in this task.
