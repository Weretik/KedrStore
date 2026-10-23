# API rules

## Contract stability

Public routes, operation names, and response fields are stable by default. Prefer compatible additions. Describe breaking changes, affected clients, migration path, and rollout order in the feature specification and contract.

For new or changed HTTP behavior, link every operation to acceptance scenarios `SC-*`. Where practical, agree on the readable contract and versioned OpenAPI document before implementing the transport layer. Create targeted contract/API Red tests from those decisions, implement the transport layer, then compare actual behavior with OpenAPI before handing it to consumers.

Use task-specific DTOs. Do not expose entities, persistence types, secrets, or internal exceptions. Define `Idempotency-Key` semantics before implementing writes that may be retried.

## Controllers

Controllers must:

- accept explicit request models;
- pass `CancellationToken` to Mediator;
- use the existing Ardalis.Result HTTP mapping;
- declare appropriate `ProducesResponseType` attributes;
- contain no validation logic, business rules, or EF code.

## Validation, results, and errors

FluentValidation validators are registered by scanning Application assemblies and run through the Mediator validation behavior. Place each validator alongside its use case.

Validate required fields, ranges, formats, enum values, lengths, and parameter combinations at the boundary. Check state and business invariants in Domain or Application use-case logic.

Return Ardalis.Result for expected errors. Preserve the existing API error mapping. Do not use exceptions for routine validation or not-found cases, and do not expose internal details in responses.
