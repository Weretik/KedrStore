# Security and observability rules

## Security and privacy

Use least privilege. The host fallback policy requires authentication; mark intentionally public endpoints with `AllowAnonymous`. Preserve existing role and policy conventions. For the model details, see [Identity and access controls](../architecture/security/README.md).

Do not log tokens, passwords, connection strings, PII, or sensitive payloads. Do not return internal exception details in production responses.

## Logs and diagnostics

Use structured Serilog messages with stable, useful properties. Preserve correlation, cancellation, and error diagnostics. Log at the appropriate level without noise.

Handle transient failures explicitly and observably. Do not introduce implicit retries for write operations that may duplicate side effects.
