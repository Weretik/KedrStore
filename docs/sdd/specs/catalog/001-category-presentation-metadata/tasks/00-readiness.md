# Phase 00 — Clarification and readiness

**Status:** completed and verified

- [x] T001 Read the frozen constants/translation snapshots, requirements, design, data model, contract, and checklist; record contradictions in this file without changing source values. **Paths:** `catalog-category-slugs.constants.ts.md`, `translate.md`, `requirements/`, `design/`, `data-model.md`, `contracts/`. **Dependencies:** none.
- [x] T002 Agree the stable matching key, complete metadata mapping, short-label limits, unconfigured-category fallback, `SortOrder` uniqueness scope, and Cosmos short labels/order. **Paths:** approved configuration source and `requirements/`. **Dependencies:** T001.
- [x] T003 Decide whether an HTTP consumer is in scope. If yes, agree and add `docs/sdd/contracts/catalog/category-presentation-metadata.openapi.yaml`, then reference it from `docs/sdd/contracts/openapi.yaml`; if no, mark API tasks not applicable and state the consuming boundary. **Paths:** `contracts/api-contract.md`, `docs/sdd/contracts/`. **Dependencies:** T002.
- [x] T004 Mark the applicable items in `checklist/spec-readiness.md`. **Dependencies:** T002, T003.

## T001 review record (2026-07-29)

Reviewed the frozen category constants and translations, all requirement and design documents, the data model, the API contract, and both readiness checklists. No source snapshot values were changed.

### Findings resolved by T002/T003

- The frozen snapshots identify category entries by `slug`, whereas the specification prohibits relying on localized labels and does not approve whether `slug`, numeric category ID, or a key scoped by `ProductTypeIdOneC` is the stable configuration key. The snapshot alone cannot resolve this because it does not contain product-type ownership for every entry.
- The snapshots provide Ukrainian labels and Russian translations, but do not define configuration records, short-label length limits, blank-value policy, unconfigured-category labels, or a deterministic fallback `SortOrder`.
- The snapshots imply presentation hierarchy and order, but do not establish the required uniqueness scope for `SortOrder` (per parent/root versus globally stable), nor prove that their displayed hierarchy always matches the final imported `ParentId`/`Path` hierarchy.
- Cosmos must receive configured root metadata, but neither frozen snapshot supplies its Ukrainian/Russian short labels or order.
- `GetCategoriesQuery` and `CategoryTreeDto` exist at the application boundary, while no HTTP endpoint, route, security policy, localization-selection rule, or OpenAPI contract exists. The feature cannot safely infer whether HTTP exposure is in scope.

The decisions below resolve T002 and T003. T004 is completed in `checklist/spec-readiness.md`.

## T002 approved decisions (2026-07-29)

- The stable configuration key is `(ProductTypeIdOneC, CategoryId)`. `CategoryId` is the existing numeric 1C/category identifier: its value is the numeric suffix of each frozen slug; `ProductTypeIdOneC` is the configured root ID. This does not use a localized name or a generated slug as a matching key.
- The approved configuration source is a typed `OneC:CategoryPresentation` options section, duplicated identically in both hosts during Phase 02. It contains one record for every root, manual group, and child listed in the frozen snapshots. Each record supplies the two short names and `SortOrder`; `Level` is calculated from the final imported `Path`/`ParentId` hierarchy and persisted, not hand-maintained in configuration.
- The complete mapping is the paired frozen inputs: `catalog-category-slugs.constants.ts.md` supplies each entry's category ID, Ukrainian short label, parent/order sequence, and hierarchy; `translate.md` supplies the matching Russian short label by the same root/section/item position. Root IDs are `5513` (hardware) and `7226` (doors), as configured by the existing `OneC` options. Manual groups are the existing IDs `900001`–`900006` and `910001`–`910002`; their child IDs are already explicitly configured in both hosts.
- `ShortNameUk` and `ShortNameRu` are required, trimmed, non-blank values with a maximum length of 100 characters. This matches the existing category-name storage limit and makes migration backfill deterministic.
- `SortOrder` is a non-negative integer unique among siblings with the same `ParentId`; root categories form the `ParentId = null` sibling set. Snapshot declaration order is zero-based within each sibling set. `Level` is zero for roots and otherwise derived from the final hierarchy.
- An unconfigured category retains its original 1C `Name`; both short-name fields receive that `Name`, and it receives `SortOrder = Int32.MaxValue` within its sibling set. Ties are ordered by existing numeric category ID. This preserves deterministic output without changing IDs, slugs, paths, or parents.
- Cosmos is the backend-owned root `(ProductTypeIdOneC = "Космос", CategoryId = 920000)`. Its short labels are `Космос` in both languages, its root `SortOrder` is `2` (after hardware `0` and doors `1`), and its level is `0`.

## T003 consumer decision (2026-07-29)

No HTTP consumer is in scope for this feature. The repository has no category endpoint that dispatches `GetCategoriesQuery`; the existing consuming boundary is the Catalog Application `GetCategoriesQuery` / `CategoryTreeDto` read model. Phase 03 may add the agreed metadata fields to that projection, while Phase 04 tasks T018–T022 are not applicable. No OpenAPI file or route is created.

## Checkpoint

Do not start Domain until T002 is complete. Do not start API until T003 has an agreed contract.
