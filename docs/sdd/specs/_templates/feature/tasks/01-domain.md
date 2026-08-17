# Phase 01 — Domain planning

> This phase only creates and orders subphases. Do not implement the entire Domain layer in this file.

- [ ] T005 Check `design/domain.md` and `data-model.md`; determine the necessary aggregate roots, value objects, invariants and permissible state transitions.
- [ ] T006 Determine whether domain events are required for side effects between aggregates; do not create them for a simple internal call.
- [ ] T007 View the subphase templates in the [Domain Templates](#domain-templates) section and select only the required Domain slices.
- [ ] T008 Copy each desired template to `tasks/domain/` as a separate file of the actual subphase: `01.1-<name>.md`, `01.2-<name>.md` and so on; replace `NN' with a number, placeholders with specific names and paths.

## Domain templates

- [01.NN — Aggregate and invariants](domain/01.NN-aggregate.template.md)
- [01.NN — Value object](domain/01.NN-value-object.template.md)
- [01.NN — Domain events](domain/01.NN-domain-events.template.md)
- [01.NN — Domain tests](domain/01.NN-domain-tests.template.md)

## Checkpoint

A separate `01.N' subphase file is created for each required Domain solution; Domain does not depend on Infrastructure.

## The next phase

After all created sub-phases of `01.N` are completed, go to [02 — Infrastructure Planning](02-infrastructure.md).
