# Phase 00 — Behavior and readiness planning

> This phase creates the behavior-first foundation and required readiness tasks.

- [ ] Confirm the feature goal, actors, scope, and exclusions.
- [ ] Assign stable `R-*` IDs to business rules and `SC-*` IDs to acceptance scenarios.
- [ ] Add relevant happy-path, negative, and boundary examples.
- [ ] Resolve `[NEEDS CLARIFICATION]` items or move them out of scope.
- [ ] Select and create only the required readiness files.
- [ ] Initialize `traceability.md` with one row per in-scope scenario.

## Readiness templates

- [Scope and scenario discovery](readiness/00.NN-scope.template.md)
- [Test strategy](readiness/00.NN-test-strategy.template.md)
- [CLI, scripts, and generators](readiness/00.NN-tooling.template.md)

## Checkpoint

Every changed behavior has a stable scenario ID and unambiguous observable
outcome. Test levels are planned, open product decisions are resolved or out of
scope, and the traceability matrix contains every scenario.

## Next planning step

Plan the technical tasks required by the scenario graph. Planning may inspect
all layers before implementation begins.
