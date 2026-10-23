# Security and observability rules

## Security and privacy

Apply least privilege. The host fallback policy requires authentication; explicitly mark intentionally public endpoints with `AllowAnonymous`. Preserve existing role and policy rules. See [Identity and access control](../architecture/security/README.md) for model details.

Do not log tokens, passwords, connection strings, personal data, or sensitive payloads. Do not return internal exception details in production responses.

## Logging and diagnostics

Use structured Serilog messages with stable, useful properties. Preserve correlation, cancellation, and error diagnostics. Choose suitable log levels without adding noise.

Handle transient failures explicitly and observably. Do not add hidden write retries that could duplicate side effects.
