# <NNN> — <feature name>

**Module:** <module>
**Type:** <feature | migration>
**Status:** draft
**Owner:** <team>
**Created:** YYYY-MM-DD

<One paragraph describing the feature's value and boundaries.>

## Specification-writing order

1. Create `docs/sdd/specs/<module>/<NNN>-<feature-slug>/` and complete this README: title, module, owner, goal, and scope.
2. Complete `requirements/overview.md`, then create a separate `requirements/<topic>.md` for each independent business domain.
3. Mark uncertainty as `[NEEDS CLARIFICATION: <question>]`; do not assume business rules, API behavior, or the data model.
4. Resolve every clarification item or explicitly move it out of scope.
5. Complete `design/domain.md`, `design/infrastructure.md`, and `data-model.md`. Requirements answer “what”; design answers “how”.
6. When an HTTP or integration consumer exists, agree operations and complete `contracts/api-contract.md` enough to implement endpoints before coding. In a code-first workflow, after API implementation create a machine-readable frontend contract at `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, reference it from `docs/sdd/contracts/openapi.yaml`, and check it against the code. Do not hand off an API consumer before the OpenAPI phase is complete.
7. Complete [Specification readiness](checklist/spec-readiness.md). Do not generate tasks while model or contract decisions remain unresolved.
8. Break implementation into concrete task IDs: each has an action, exact path, dependencies, and a checkpoint. Main phases `00`–`05` only create the required subphases. Copy templates to `tasks/readiness/00.N-<name>.md`, `tasks/domain/01.N-<name>.md`, `tasks/infrastructure/02.N-<name>.md`, `tasks/application/03.N-<name>.md`, `tasks/api/04.N-<name>.md`, or `tasks/verification/05.N-<name>.md`. Do not create files for excluded slices or use cases. For HTTP/integration work, execute API subphases in order: controllers, HTTP behavior, OpenAPI, API tests.
9. Complete phases in sequence, mark completed tasks as `[x]`, and record verification results in the documentation.
10. Before completion, complete [Delivery readiness](checklist/delivery-readiness.md), align the README, requirements, design, and contracts with the code, and prepare the delivery report.

## CLI-first rule

Use an existing project CLI command, generator, or script instead of creating
artifacts manually: migrations, SQL scripts, OpenAPI validation/generation,
restore, build, tests, and formatting/linting. Find the accepted repository
command first; do not invent automation where the project already has one.
Manual changes are allowed only for logic, contracts, or exceptions that a
generator cannot express correctly, with the reason recorded in the task.

## Small-subphase rule

Main phases `00 — Readiness`, `01 — Domain`, `02 — Infrastructure`,
`03 — Application`, `04 — API`, and `05 — Verification` orchestrate work only:
they determine the needed subphases and their order. Do not put a large
implementation task or every detail in a main phase file.

Each concrete responsibility has its own subphase file: scope/decisions/tooling;
an aggregate, value object, events, or tests; persistence, migration, read
model, integration, or tests; contracts or one completed application use case;
controllers, HTTP behavior, OpenAPI, or API tests; and build/tests or delivery
documentation. One subphase file has one clear responsibility.

## Requirements

- [Overview and scope](requirements/overview.md)
- [<business domain>](requirements/topic.md)

## Technical design

- [Domain](design/domain.md)
- [Infrastructure](design/infrastructure.md)
- [Data model](data-model.md)

## Delivery

- [API/integration contract](contracts/api-contract.md)
- [Machine-readable API contract registry](../../../contracts/README.md)
- [Specification readiness](checklist/spec-readiness.md)
- [Delivery readiness](checklist/delivery-readiness.md)

## AI implementation tasks

AI performs only unfinished tasks in the current phase, marks completed tasks
as `[x]`, and proceeds only after the checkpoint. `[P]` indicates safe parallel
execution after dependencies are complete. See the [phase-file navigation](tasks/README.md).

### Required phase order for an HTTP/API feature

After Domain, Infrastructure, and all agreed Application use cases, create
separate numbered subphases from `tasks/api/` in this order:

1. `NN-api-controllers.md` — inspect `Program.cs`/the composition root, enable controllers only when needed, and create `<Feature>Controller` plus an endpoint for each agreed use case.
2. `NN-api-http-behavior.md` — add API contracts and mappers only when needed, configure authentication/authorization, and use the existing `Ardalis.Result` → `ToActionResult` mapping.
3. `NN-api-contract-documentation.md` — document actual endpoints in `api-contract.md` and versioned OpenAPI YAML.
4. `NN-api-tests.md` — add HTTP/integration tests, including authorization.
5. `NN-verification.md` — run build, tests, the delivery checklist, and prepare the delivery report.

Do not skip these API subphases for a feature with an HTTP endpoint or
integration API. Do not create them for a feature without an HTTP/integration
surface.

| Phase | Result |
| --- | --- |
| [00 — Readiness](tasks/00-readiness.md) | agreed scope, CQRS use cases, and contract outline |
| [01 — Domain planning](tasks/01-domain.md) | required and excluded Domain slices |
| [02 — Infrastructure planning](tasks/02-infrastructure.md) | required and excluded Infrastructure slices |
| [01.NN — Domain: aggregate](tasks/domain/01.NN-aggregate.template.md) | aggregate root, invariants, and unit tests |
| [01.NN — Domain: value object](tasks/domain/01.NN-value-object.template.md) | immutable value object and unit tests |
| [01.NN — Domain: events](tasks/domain/01.NN-domain-events.template.md) | events only for cross-aggregate side effects |
| [01.NN — Domain: tests](tasks/domain/01.NN-domain-tests.template.md) | complete coverage of changed Domain rules |
| [02.NN — Infrastructure: persistence](tasks/infrastructure/02.NN-persistence-mapping.template.md) | EF mapping, constraints, indexes, and tests |
| [02.NN — Infrastructure: migration](tasks/infrastructure/02.NN-migration.template.md) | verified migration and rollout/rollback |
| [02.NN — Infrastructure: read model](tasks/infrastructure/02.NN-read-model.template.md) | query service/read repository and tests |
| [02.NN — Infrastructure: integration/outbox](tasks/infrastructure/02.NN-integration-outbox.template.md) | external integration, delivery, and retry |
| [02.NN — Infrastructure: tests](tasks/infrastructure/02.NN-infrastructure-tests.template.md) | integration tests for critical behavior |
| [03 — Application](tasks/03-application.md) | plan for subphases 03.1, 03.2, … |
| [03.NN — Application subphase](tasks/application/) | contracts or one complete use case with validation, results, and tests |
| [04 — API](tasks/04-api.md) | plan for subphases 04.1, 04.2, … |
| [04.NN — API subphase](tasks/api/) | controllers, HTTP behavior, OpenAPI, or API tests |
| [05 — Verification](tasks/05-verification.md) | build, tests, delivery checklist, and report |
