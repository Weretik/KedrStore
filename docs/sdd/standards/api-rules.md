# API rules

## Contract stability

Public routes, operation names, and response fields are stable by default. Prefer compatible additions. Describe breaking changes, affected clients, migration path, and rollout order in the feature specification and contract.

For new or changed HTTP behavior, link every operation to acceptance scenarios `SC-*`. Where practical, agree on the readable contract and versioned OpenAPI document before implementing the transport layer. Create targeted contract/API Red tests from those decisions, implement the transport layer, then compare actual behavior with OpenAPI before handing it to consumers.

Use task-specific DTOs. Do not expose entities, persistence types, secrets, or internal exceptions. Define `Idempotency-Key` semantics before implementing writes that may be retried.

## Endpoints

Modules expose Minimal API endpoints, never controllers ([module rules](module-rules.md), Api layer). An endpoint must:

- live in `<Module>.Api/Endpoints/<Feature>/` as one `<UseCase>Endpoint` class (`Map` + `Handle`) mapped by the feature's `<Feature>Endpoints` group, which sets the tag and access policy;
- bind an explicit request record (body, or query through `[AsParameters]` with nullable values), never an Application command or query, and map it in `Handle`;
- pass `CancellationToken` to Mediator;
- answer through the one result mapper (`ToHttpResult` / `ToProblem`);
- declare its answers (`.Produces<T>()`, `.ProducesProblem(4xx)`, `.Produces(5xx)`) and its name (`.WithName`, the operation id);
- contain no validation logic, business rules, or EF code.

## Validation, results, and errors

FluentValidation validators are registered by scanning Application assemblies and run through the Mediator validation behavior. Place each validator alongside its use case.

Validate required fields, ranges, formats, enum values, lengths, and parameter combinations at the boundary. Check state and business invariants in Domain or Application use-case logic.

Return Ardalis.Result for expected errors; the one [BuildingBlocks.Api mapper](../architecture/platform/building-blocks/api.md) answers them as `ProblemDetails` with a `code` extension. Do not build error bodies in an endpoint. Do not use exceptions for routine validation or not-found cases, and do not expose internal details in responses.

Expected business errors carry a stable snake_case `code` the client branches on. When a staff client shows the message as is, write it in Ukrainian for the manager: name the product, customer, or field, state the allowed limit, and say what to change (for example `price_below_minimum`: «Ціна «Ручка» після знижки 0,50 ₴, а має бути щонайменше 1,00 ₴ за шт. Найбільша знижка — 156,50 ₴.»). List such codes in the operation's contract so the client knows which messages it may show directly; other codes keep a technical message and the client words them itself. Limits the client also checks (ranges, minimum prices) live in one named constant and match the client's model.
