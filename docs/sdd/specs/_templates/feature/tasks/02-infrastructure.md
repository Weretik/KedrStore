# Phase 02 — Infrastructure and enabler task planning

> Create only the Infrastructure `TS-*` and shared `EN-*` files required by
> scenarios.

- [ ] Review scenarios, `design/infrastructure.md`, and `data-model.md`.
- [ ] Identify persistence, constraints, migrations, read models, integrations,
      outbox behavior, configuration, and operational prerequisites.
- [ ] Use `TS-*` for directly testable behavior and `EN-*` for shared or
      prerequisite work with a documented test-first exception.
- [ ] Add `Covers` or `Enables`, dependencies, exact paths, verification, and a
      checkpoint to every created task.
- [ ] Map every task into `traceability.md`.

## Templates

- [Shared enabler](enablers/EN-NNN-enabler.template.md)
- [Persistence mapping](infrastructure/02.NN-persistence-mapping.template.md)
- [Migration](infrastructure/02.NN-migration.template.md)
- [Read model](infrastructure/02.NN-read-model.template.md)
- [Integration or outbox](infrastructure/02.NN-integration-outbox.template.md)
- [Infrastructure coverage](infrastructure/02.NN-infrastructure-tests.template.md)

## Checkpoint

Every required Infrastructure responsibility or shared prerequisite is mapped
to scenarios. Applied migrations are not edited, generated work uses the
repository CLI, and every `EN-*` exception has replacement verification.
