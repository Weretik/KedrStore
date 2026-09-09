# Phase 03 — Application task planning

> Create one small `TS-*` file per use case or shared Application responsibility.

- [ ] Derive Application use cases from `SC-*` scenarios rather than CRUD assumptions.
- [ ] Select only templates that match agreed behavior.
- [ ] Give each task a `TS-*` ID, covered scenarios, dependencies, exact paths,
      test level, and checkpoint.
- [ ] Keep validation, result behavior, orchestration, and focused tests in the
      same use-case task where practical.
- [ ] Map every Application task into `traceability.md`.

## Templates

- [Contracts and data access](application/03.NN-contracts.template.md)
- [Get list](application/03.NN-get-list.template.md)
- [Get by id](application/03.NN-get-by-id.template.md)
- [Create](application/03.NN-create.template.md)
- [Update](application/03.NN-update.template.md)
- [Delete](application/03.NN-delete.template.md)
- [Other use case](application/03.NN-other-use-case.template.md)

## Checkpoint

Every agreed use case is connected to its acceptance scenarios. Tasks do not
duplicate Domain rules or introduce HTTP concerns.
