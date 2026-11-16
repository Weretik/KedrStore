# Diagnostics: health, logs, and Swagger

## Health check

`GET /health` is configured in Host.Api with explicit anonymous access. Check it first to determine whether the host is running and reachable.

## Swagger and OpenAPI

Swagger UI is available only when `ASPNETCORE_ENVIRONMENT=Development`. With the HTTPS profile, open:

```text
https://localhost:7230/swagger
```

Swagger UI reads the built-in document `/openapi/v1.json`, the same one the build writes to `docs/sdd/contracts/openapi.json`. The bearer scheme expects an access token from `POST /api/auth/mobile/login`. Supply the access token to Swagger authorization; do not paste or expose a refresh token.

For reproducible feature verification, create an API-test task in the relevant specification. Record the route, prerequisites, input, expected status, errors, authorization cases, and fresh evidence. Choose the test level according to the [testing rules](../../standards/testing-rules.md).

## Logs and startup failures

Serilog is configured through the host configuration and writes to the console: plain text locally, Cloud Logging JSON in production (one object per line with `severity` and the trace id). HTTP request logging is enabled in the pipeline. If `Host.Api` does not start, check its log and configuration first. Migrations and seeders run through a separate `Host.Jobs --job=migrate`, so check that job's logs for their result; the [database preparation order](../data/migrations-and-seeding.md) is documented separately.

Do not log or share tokens, passwords, complete connection strings, cookie values, or raw sensitive payloads. Before changing code, distinguish these protected-route failures:

| Symptom | First check |
| --- | --- |
| `401 Unauthorized` | An access token exists, is valid, and is sent as a bearer token. |
| `403 Forbidden` | The authenticated user has the role or authorization policy required by the endpoint. |
| Browser blocks the request | Allowed CORS origin, HTTPS/cookie attributes, and CSRF header for refresh. |
| API does not serve requests | Required settings, API log, and dependency availability; check schema and seed state separately through `Host.Jobs --job=migrate`. |

## Traces in Google Cloud

In production every API request is a trace in Cloud Trace, service `kedr-api` (the Cloud Run service itself is named `api`) ([spec platform/008](../../specs/platform/008-request-tracing/README.md), [ADR-0014](../../architecture/decisions/0014-opentelemetry-traces-to-cloud-trace.md)). A trace shows the endpoint, each SQL command and outgoing call (1C, Telegram, CDN) with its duration, and the exception if one was thrown.

**From a log line to the trace.** Cloud Console → Logging → Logs Explorer, query `resource.type="cloud_run_revision" resource.labels.service_name="api" severity>=ERROR`. Open a line and choose «View trace details» next to its trace id.

**From a trace to its logs.** Cloud Console → Trace → Trace Explorer, filter service `kedr-api` (or status error, or latency above a value), open a trace and choose «View logs».

**No spans from the API.** When the trace has only the Cloud Run spans (`AppServer`, `HTTP load balancer`) and nothing from the service, the export fails. The SDK's own warnings and errors are in the API log under logger `BuildingBlocks.Observability.OpenTelemetryErrorLog` (for example 403 from the Telemetry API); query `jsonPayload.logger="BuildingBlocks.Observability.OpenTelemetryErrorLog"`.

**From the command line** (read-only):

```bash
gcloud logging read 'resource.type="cloud_run_revision" AND resource.labels.service_name="api" AND severity>=ERROR' \
  --project classkedrproject --freshness 1d --limit 20 --format json
# every entry has trace: projects/classkedrproject/traces/<id>; all lines of that request:
gcloud logging read 'trace="projects/classkedrproject/traces/<id>"' --project classkedrproject
```

**One-time setup in the project** (owner, project `classkedrproject`): enable the APIs `telemetry.googleapis.com` and `cloudtrace.googleapis.com`; give the Cloud Run runtime account of the service `api` the role Cloud Trace Agent (`roles/cloudtrace.agent`). Until then the API works and only the traces are missing; the JSON logs work without this step.

**Settings:** `Observability:GoogleCloudProjectId` turns export and JSON logs on (production only); `Observability:TraceSampleRatio` (0…1) keeps that share of traces if the free tier is ever exceeded; `Observability:ServiceName` names the service in Cloud Trace.
