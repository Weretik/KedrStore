# API rules

## Contract stability

Public routes, action names, and response fields are stable by default. Prefer additive changes. Document an incompatible change, affected clients, migration path, and rollout plan in the feature specification and contract.

Use task-specific DTOs. Do not expose entities, persistence types, secrets, or internal exceptions. For write operations that risk repeated execution, define `Idempotency-Key` semantics before implementation.

## Controllers

Controllers must:

- accept explicit request models;
- pass `CancellationToken` to Mediator;
- return the established Ardalis.Result HTTP mapping;
- declare relevant `ProducesResponseType` attributes;
- contain no validation, business, or EF logic.

## Validation, results, and errors

FluentValidation validators are registered by scanning the Application assembly and run through the Mediator validation behavior. Place the validator in the folder of its corresponding use case.

Validate input form: required fields, ranges, formats, enum values, length, and input combinations. Check state and business invariants in Domain or Application use-case logic.

Return Ardalis.Result for expected failures. Preserve the current API error mapping. Do not use exceptions for ordinary validation or not-found control flow, and do not expose internal details in responses.
