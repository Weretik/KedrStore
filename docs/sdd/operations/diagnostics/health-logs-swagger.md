# Diagnostics: health, logs, and Swagger

## Health check

`GET /health` is configured in Host.Api with explicit anonymous access. Check it first to determine whether the host is running and reachable.

## Swagger and OpenAPI

Swagger UI is available only when `ASPNETCORE_ENVIRONMENT=Development`. With the HTTPS profile, open:

```text
https://localhost:7230/swagger
```

The Swagger bearer scheme expects an access token from `POST /api/auth/session/login`. Supply the access token to Swagger authorization; do not paste or expose a refresh token.

For reproducible feature verification, create an API-test task in the relevant specification. Record the route, prerequisites, input, expected status, errors, authorization cases, and fresh evidence. Choose the test level according to the [testing rules](../../standards/testing-rules.md).

## Logs and startup failures

Serilog is configured through the host configuration and writes to the console. HTTP request logging is enabled in the pipeline. If `Host.Api` does not start, check its log and configuration first. Migrations and seeders run through a separate `Host.Jobs --job=migrate`, so check that job's logs for their result; the [database preparation order](../data/migrations-and-seeding.md) is documented separately.

Do not log or share tokens, passwords, complete connection strings, cookie values, or raw sensitive payloads. Before changing code, distinguish these protected-route failures:

| Symptom | First check |
| --- | --- |
| `401 Unauthorized` | An access token exists, is valid, and is sent as a bearer token. |
| `403 Forbidden` | The authenticated user has the role or authorization policy required by the endpoint. |
| Browser blocks the request | Allowed CORS origin, HTTPS/cookie attributes, and CSRF header for refresh. |
| API does not serve requests | Required settings, API log, and dependency availability; check schema and seed state separately through `Host.Jobs --job=migrate`. |
