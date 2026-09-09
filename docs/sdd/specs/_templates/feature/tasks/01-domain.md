# Phase 01 — Domain task planning

> Create only the Domain `TS-*` files required by scenarios. Do not implement
> the whole feature in this orchestration file.

- [ ] Review scenario rules, `design/domain.md`, and `data-model.md`.
- [ ] Identify aggregates, value objects, invariants, and state transitions.
- [ ] Determine whether domain events are required for cross-aggregate effects.
- [ ] Create one small Domain task per responsibility from the relevant template.
- [ ] Give each task a `TS-*` ID, `Covers: SC-*`, dependencies, exact paths, test
      level, and checkpoint.
- [ ] Map every Domain task into `traceability.md`.

## Templates

- [Aggregate and invariants](domain/01.NN-aggregate.template.md)
- [Value object](domain/01.NN-value-object.template.md)
- [Domain events](domain/01.NN-domain-events.template.md)
- [Domain coverage](domain/01.NN-domain-tests.template.md)

## Checkpoint

Every required Domain responsibility has one traceable technical task. Domain
does not depend on Application, Infrastructure, or API. Scenarios without new
Domain behavior explicitly use `—` in the matrix.
