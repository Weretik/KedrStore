# API contracts

[`openapi.json`](openapi.json) is the only machine-readable contract of the public HTTP API. It is **generated from the code**: every build of `Host.Api` writes it ([ADR-0013](../architecture/decisions/0013-openapi-generated-from-code.md)). Do not edit it by hand.

## How it works

1. An endpoint describes itself in code: `.WithName("getAdminArticles")` (unique camelCase operation id, the frontends see it), `.WithSummary("List articles")`, typed `.Produces<TResponse>()` and `.ProducesProblem(status)` for every error it returns, and its request type through the handler parameters.
2. `dotnet build` (or any build or test of `Host.Api`) regenerates `openapi.json`. Commit it together with the endpoint change.
3. CI builds the solution and fails with «OpenAPI contract is out of date» when the committed file differs from the code.
4. The architecture test [`OpenApiDocumentRulesTests`](../../../tests/ArchitectureTests/Hosts/OpenApiDocumentRulesTests.cs) requires an operation id, a summary and a tag on every operation, unique ids, and numbers documented as numbers.
5. kedr-web downloads `openapi.json` from `master` (`npm run contracts:sync`), generates types with `openapi-typescript` and calls the API through `openapi-fetch`. Its CI fails when its copy is behind `master`, and a daily job opens a sync PR. A breaking change of a path, parameter or schema becomes a TypeScript error there.

The document describes the code; it does not grant access. Authorization policies and the host fallback policy decide who may call an endpoint.

## Errors

Every error is `application/problem+json` with `code`, `detail` and `traceId` (the [frontend integration guide](frontend-integration.md) lists the codes). Validation errors add `errors` by field. A few public reads answer 404 with a typed redirect body (`CatalogRedirect`); they declare it with `Produces<T>(404)`.

## Changing the API

- Record decisions a person must read (scope, consumers, compatibility, security, errors, idempotency, deployment order) in the feature's `contracts/api-contract.md`.
- Write the API test first (`tests/IntegrationTests/<Module>/Api/`), then the endpoint; the regenerated `openapi.json` shows the contract change in review.
- A breaking change (removed or renamed path, parameter or field, changed type) needs a migration path for the frontends and the mobile app, which may run an old version.
- In the PR say that kedr-web must sync the contract; its daily job also opens the sync PR on its own.

## Frontend handoff

- [Frontend integration guide](frontend-integration.md): authentication, CSRF, pagination, errors, and endpoint owners.
- Swagger UI in Development: `/swagger`, reading `/openapi/v1.json`.
