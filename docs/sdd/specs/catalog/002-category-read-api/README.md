# 002 — Category read API

**Module:** Catalog
**Type:** feature
**Status:** specified
**Owner:** Catalog team
**Created:** 2026-07-31

Expose the backend-owned category navigation model to the storefront through public read endpoints. The contract gives the frontend a localized, deterministic category tree and a single-category view, including identity, slug, parent relation, display order, depth, descendants, and breadcrumbs. It does not change category import, persistence, or any frontend UI.

## Specification-writing order

1. Create `docs/sdd/specs/<module>/<NNN>-<feature-slug>/` and complete this `README.md`: title, module, owner, goal, and scope.
2. Complete `requirements/overview.md`, then create a separate `requirements/<topic>.md` for each independent business domain.
3. Mark uncertainty as `[NEEDS CLARIFICATION: <question>]`; do not assume business rules, API behavior, or the data model.
4. Resolve every `[NEEDS CLARIFICATION]` item or explicitly move it out of the feature scope.
5. Complete `design/domain.md`, `design/infrastructure.md`, and `data-model.md`. Requirements answer “what”; design answers “how”.
6. If an HTTP consumer exists, complete `contracts/api-contract.md`, agree `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, and add it to `docs/sdd/contracts/openapi.yaml` before writing API code. Document non-HTTP integration contracts in `contracts/api-contract.md`; do not create OpenAPI solely for an internal integration.
7. Complete [Specification readiness](checklist/spec-readiness.md). Do not create tasks while the specification contains unresolved model or contract decisions.
8. Break implementation into concrete task IDs in `tasks/00-readiness.md`–`tasks/05-verification.md`: each task has an action, exact path, dependencies, and a phase checkpoint.
9. Complete phases in sequence, mark completed tasks as `[x]`, and record verification results in the documentation.
10. Before completion, complete [Delivery readiness](checklist/delivery-readiness.md), align the README, requirements, design, and contracts with the code, and prepare the delivery report.

## Requirements

- [Overview and scope](requirements/overview.md)
- [Category navigation](requirements/category-navigation.md)
- [Admin category read](requirements/admin-category-read.md)

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
| [00 — Clarification](tasks/00-readiness.md) | agreed scope and contract |
| [01 — Domain](tasks/01-domain.md) | model and unit tests |
| [02 — Infrastructure](tasks/02-infrastructure.md) | persistence and migration |
| [03 — Application](tasks/03-application.md) | CQRS use cases |
| [04 — API](tasks/04-api.md) | endpoints and API tests |
| [05 — Verification](tasks/05-verification.md) | delivery evidence |
