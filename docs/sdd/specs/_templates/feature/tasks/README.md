# Phases of feature implementation

Start with `00-readiness.md`. All main phases `00–05` are orchestration only: create details with separate files of subphases `00.1`, `01.1`... `05.1` in the corresponding folders. In sub-phases, replace `NN' with an actual number and assign unique sequential task IDs.

Do not add the entire implementation to the main phases: each specific task must be a separate `00.N`, `01.N`, `02.N`, `03.N`, `04.N` or `05.N` file.

## Mandatory start

- [00 — Refinement and readiness](00-readiness.md)
- [01 — Domain Planning](01-domain.md)
- [02 — Infrastructure Planning](02-infrastructure.md)

## Readiness — only required subphases

- [00.NN — Scope, solutions and CQRS use cases](readiness/00.NN-scope.template.md)
- [00.NN — CLI, scripts and generators](readiness/00.NN-tooling.template.md)

## Domain — only required slices

- [01.NN — Aggregate and invariants](domain/01.NN-aggregate.template.md)
- [01.NN — Value object](domain/01.NN-value-object.template.md)
- [01.NN — Domain events](domain/01.NN-domain-events.template.md)
- [01.NN — Domain tests](domain/01.NN-domain-tests.template.md)

## Infrastructure — only required slices

- [02.NN — Persistence mapping](infrastructure/02.NN-persistence-mapping.template.md)
- [02.NN — Migration via EF Core CLI](infrastructure/02.NN-migration.template.md)
- [02.NN — Read model](infrastructure/02.NN-read-model.template.md)
- [02.NN — Integration or outbox](infrastructure/02.NN-integration-outbox.template.md)
- [02.NN — Infrastructure tests](infrastructure/02.NN-infrastructure-tests.template.md)

## Application — only required subphases

- [03 — Application](03-application.md)
- [03.NN — Application contracts](application/03.NN-contracts.template.md)
- [03.NN — Get list](application/03.NN-get-list.template.md)
- [03.NN — Get by id](application/03.NN-get-by-id.template.md)
- [03.NN — Create](application/03.NN-create.template.md)
- [03.NN — Update](application/03.NN-update.template.md)
- [03.NN — Delete](application/03.NN-delete.template.md)
- [03.NN — Other use case](application/03.NN-other-use-case.template.md)

Each Application use case contains its own validation, `Ardalis.Result' and unit tests.

## API - only required subphases for HTTP/integration feature

Create the required subphases after all agreed Application use cases in the following order:

1. [04 — API](04-api.md)
2. [04.NN — Controllers and endpoints](api/04.NN-controllers.template.md)
3. [04.NN — HTTP contracts, rights and Result mapping](api/04.NN-http-behavior.template.md)
4. [04.NN — OpenAPI documentation](api/04.NN-contract-documentation.template.md)
5. [04.NN — API/integration tests](api/04.NN-tests.template.md)
6. [05 — Verification](05-verification.md)

Don't create feature-only API phases without an HTTP/integration surface.

## Verification - only required subphases

- [05 — Verification](05-verification.md)
- [05.NN — Build and automated tests](verification/05.NN-build-tests.template.md)
- [05.NN — Delivery documentation and report](verification/05.NN-delivery.template.md)
