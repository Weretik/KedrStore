# ADR-0013: The OpenAPI contract is generated from the code at build

- Status: accepted
- Date: 2026-11-15 (owner's request)
- Owner: Виталий (project owner)

## Context

The public API contract was written by hand as 36 YAML files in `docs/sdd/contracts/`. The code already produced its own OpenAPI document (`Microsoft.AspNetCore.OpenApi`, plus Swashbuckle for Swagger UI), but nothing tied the two together, so the hand-written files could drift from the endpoints. kedr-web copied the YAML by hand with `contracts:sync` and generated its types from it, so a drift reached the frontends unnoticed.

## Alternatives considered

- Keep hand-written YAML and compare it with the generated document in a test: rejected; two sources still have to be edited for every change, and a structural comparison of two documents is fragile.
- Swashbuckle as the generator: rejected; the framework generator is already registered, supports .NET 10 and runs at build time, so a second generator only duplicates it.
- Generate the document in a test (`WebApplicationFactory`): rejected as the main path; the build-time generator writes the file with no extra step, and CI only compares it.

## Decision

- The build of `Host.Api` writes the document of the code to [`docs/sdd/contracts/openapi.json`](../../contracts/openapi.json) through `Microsoft.Extensions.ApiDescription.Server` (`OpenApiGenerateDocuments`). It is the only machine-readable API contract; the hand-written YAML files are removed.
- CI builds the solution and fails when the committed `openapi.json` differs from the generated one: commit the regenerated file with every endpoint change.
- Each endpoint describes itself in code: `WithName` (a unique camelCase operation id), `WithSummary`, typed `Produces<T>` and `ProducesProblem` for errors. The architecture test `OpenApiDocumentRulesTests` requires an operation id, a summary and a tag on every operation and forbids `integer|string` numbers.
- Host transformers shape the document: `StrictNumberSchemaTransformer` documents numbers as numbers, `BearerSecuritySchemeTransformer` documents the mobile token. Swagger UI (Development) reads `/openapi/v1.json`; Swashbuckle's generator is removed.
- Decisions a person must read (consumers, compatibility, idempotency, errors) stay in the feature's `contracts/api-contract.md`.
- Frontends download `openapi.json` from `master` of this repository, generate types with `openapi-typescript` and call the API through `openapi-fetch`; their CI fails when their copy is behind (kedr-web ADR).

## Why

- The contract cannot drift from the code: it is the code's own description, checked by CI.
- One change of an endpoint is one edit; the frontend sees a breaking change as a type error.

## Trade-offs

- Every build starts the host once (about 3 seconds) without a database; the host removes background services for that run. The container image build skips it.
- Text descriptions and examples of the old YAML are gone; only summaries moved into the code.
- Schema names follow the C# types (`AdminArticleSummaryResponse`), so the frontends renamed their type references once.

## Sources and checks

- [Contracts](../../contracts/README.md), `src/Bootstrapper/Host.Api/Host.Api.csproj`, `src/Bootstrapper/Host.Api/Http/OpenApiRegistrationExtensions.cs`, `tests/ArchitectureTests/Hosts/OpenApiDocumentRulesTests.cs`, the CI step «Check the OpenAPI contract is up to date».
- The same decision applies to BreviERP.
