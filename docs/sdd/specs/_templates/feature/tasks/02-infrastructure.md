# Phase 02 — Infrastructure planning

> This phase only creates and orders subphases. Do not implement the entire Infrastructure layer in this file.

- [ ] T009 Check `design/infrastructure.md` and `data-model.md`; define the required persistence mapping, constraints, indexes, migration and read model.
- [ ] T010 Determine whether there is an external integration, integration event, outbox, file, cache, or other Infrastructure adapter in scope.
- [ ] T011 View the subphase templates in the [Infrastructure Templates](#infrastructure-templates) section and select only the necessary Infrastructure slices.
- [ ] T012 Copy each required template to `tasks/infrastructure/` as a separate file of the actual subphase: `02.1-<name>.md`, `02.2-<name>.md` and so on; replace `NN' with a number, placeholders with specific names and paths.

## Templates Infrastructure

- [02.NN — Persistence mapping](infrastructure/02.NN-persistence-mapping.template.md)
- [02.NN — Migration via EF Core CLI](infrastructure/02.NN-migration.template.md)
- [02.NN — Read model](infrastructure/02.NN-read-model.template.md)
- [02.NN — Integration or outbox](infrastructure/02.NN-integration-outbox.template.md)
- [02.NN — Infrastructure tests](infrastructure/02.NN-infrastructure-tests.template.md)

## Checkpoint

A separate `02.N' subphase file is created for each required Infrastructure solution; applied migrations are not edited.

## The next phase

After all created sub-phases of `02.N` are completed, go to [03 - Application](03-application.md).
