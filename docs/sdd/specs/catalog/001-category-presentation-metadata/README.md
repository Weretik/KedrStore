# Category presentation metadata

**Module:** catalog
**Type:** feature
**Status:** verified
**Owner:** backend team
**Created:** 2026-07-27

The Catalog backend will own localized category presentation metadata—short Ukrainian and Russian labels, display order and hierarchy level—while retaining the original 1C name and all existing identifiers. The feature also preserves the backend-owned virtual Cosmos category so that Cosmos product import never calls the unavailable 1C categories operation. It does not change storefront code, public routes, generated SOAP code, or existing category IDs and slugs.

## Source baseline

- [Frozen storefront category constants](catalog-category-slugs.constants.ts.md) define the current slugs, Ukrainian labels, nesting and display order.
- [Frozen UA/RU translations](translate.md) define the current Russian short labels.
- Current 1C import configuration and the existing virtual Cosmos category are the implementation baseline; the snapshots are inputs, not runtime source code.

## Requirements

- [Overview and scope](requirements/overview.md)
- [Category presentation metadata](requirements/category-presentation.md)
- [Cosmos import safety](requirements/cosmos-import.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)

## Delivery

- [API/integration contract](contracts/api-contract.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## AI implementation tasks

AI performs only unfinished tasks in the current phase, marks completed tasks as `[x]`, and proceeds only after the checkpoint. `[P]` indicates safe parallel execution after dependencies are complete.

| Phase | Result |
| --- | --- |
| [00 — Clarification](tasks/00-readiness.md) | agreed metadata mapping and delivery contract |
| [01 — Domain](tasks/01-domain.md) | aggregate metadata and unit tests |
| [02 — Infrastructure](tasks/02-infrastructure.md) | configuration, persistence and migration |
| [03 — Application](tasks/03-application.md) | import behavior and read projection |
| [04 — API](tasks/04-api.md) | agreed category-tree consumer contract, if applicable |
| [05 — Verification](tasks/05-verification.md) | delivery evidence and rollout record |
