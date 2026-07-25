# Diagnostics: health, logs and Swagger

## Health

`GET /health` is mapped by Host.Api and explicitly allows anonymous access. Use it first to establish that the host is running and reachable.

## Swagger and OpenAPI

Swagger UI is enabled only when `ASPNETCORE_ENVIRONMENT=Development`. With the HTTPS launch profile, open:

```text
https://localhost:7230/swagger
```

The Swagger bearer scheme expects the access token returned by `POST /api/auth/session/login`. Paste the access token into Swagger authorization; do not paste or expose a refresh token.

For a feature-specific reproducible test, fill in [the Swagger manual-test phase](../../specs/_templates/feature/phases/05-swagger-manual-test.md). It must record route, prerequisites, input, expected status and error cases.

## Logs and startup failures

Serilog is configured from host configuration and writes to the console. Request logging is enabled in the HTTP pipeline. On startup failure, first inspect console output for configuration validation, database migration or seeder failures.

Do not log or share tokens, passwords, full connection strings, cookie values or raw sensitive payloads. For a protected-route issue, distinguish these cases before changing code:

| Observation | First check |
| --- | --- |
| `401 Unauthorized` | Access token exists, is current and is sent as bearer token. |
| `403 Forbidden` | Authenticated user has the role/policy required by the endpoint. |
| Browser request blocked | Allowed CORS origin, HTTPS/cookie attributes and CSRF header for refresh. |
| API fails before serving requests | Database connectivity, migrations, seeders and required settings. |
