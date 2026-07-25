# API and contracts

Public routes, action names and response fields are stable by default. Prefer additive changes. Document a breaking contract change, affected clients, migration path and rollout plan in the feature specification.

Controllers must:

- accept explicit request models;
- pass CancellationToken to Mediator;
- return the established Ardalis.Result HTTP mapping;
- declare relevant ProducesResponseType attributes;
- contain no validation/business/EF logic.

Use task-specific DTOs. Do not expose entities, persistence types, secrets or internal exceptions. For a retry-sensitive write, define Idempotency-Key semantics before implementation.
