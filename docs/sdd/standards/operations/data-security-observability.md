# Data, security and observability

Reads use projections and AsNoTracking where appropriate. Avoid N+1 queries, unbounded list endpoints and unnecessary materialization. Do not create a migration unless the feature explicitly requires one.

Respect existing transaction and concurrency boundaries. Do not add automatic retries to side-effecting writes without documented idempotency.

Use least privilege. The host fallback policy requires authentication; annotate intentionally public endpoints with AllowAnonymous. Preserve existing role/policy conventions. For the concrete authentication and authorization model, see [Identity and access control](../../architecture/security/README.md).

Use structured Serilog messages with useful stable properties. Never log tokens, passwords, connection strings or sensitive payloads. Preserve cancellation and error diagnostics.
