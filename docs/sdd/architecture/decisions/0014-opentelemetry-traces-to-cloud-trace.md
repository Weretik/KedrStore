# ADR-0014: Requests are traced with OpenTelemetry straight to Google Cloud Trace

- Status: accepted
- Date: 2026-11-16 (owner's request: «если нет, нужно в проекты ввести трейсы»)
- Owner: Виталий (project owner)

## Context

The API runs on Cloud Run, which already sends the console output to Cloud Logging. Before this decision the host had no tracing, and Serilog wrote plain text, so Cloud Logging stored lines without a severity and without any link to their request. Finding why a request failed meant guessing by time stamps. Spec: [platform/008](../../specs/platform/008-request-tracing/README.md).

## Alternatives considered

- **Google's legacy libraries** (`Google.Cloud.Diagnostics.AspNetCore3`): rejected; they predate OpenTelemetry, Google now points new code to OpenTelemetry and the OTLP endpoint, and they do not trace Npgsql.
- **OpenTelemetry Collector as a Cloud Run sidecar**: rejected for now; a second container on every instance, a config file and more memory for two small services. The app can export to the same endpoint itself; a collector can be added later without code changes in the modules.
- **Another tracing backend (Jaeger, Grafana, Seq)**: rejected; a server to run and pay for, while Cloud Trace is in the same project, joins the Cloud Run request logs and has a free tier.
- **Logs only, with a correlation id**: rejected; it does not show where the time went (SQL, 1C, Telegram) and needs hand-written plumbing in every module.

## Decision

- `Host.Api` uses OpenTelemetry (`BuildingBlocks.Observability`, referenced only by hosts): ASP.NET Core, HttpClient and Npgsql instrumentation, exceptions recorded on spans, `/health` excluded.
- The OTLP exporter sends over HTTP/protobuf straight to the Google Telemetry API `https://telemetry.googleapis.com/v1/traces`, with the bearer token of the Cloud Run runtime account (Application Default Credentials). The resource carries `service.name` and `gcp.project_id`.
- A ratio sampler by trace id (`Observability:TraceSampleRatio`, default 1) ignores the incoming sampled flag of the Cloud Run front end but keeps its trace id.
- In production the console log is Cloud Logging JSON with `logging.googleapis.com/trace` and `spanId`, so every log line links to its trace, and `stack_trace` feeds Error Reporting.
- Export and JSON logs turn on only with `Observability:GoogleCloudProjectId` (set in `appsettings.Production.json`); development and tests are unchanged.
- The runtime account needs `telemetry.traces.write` (role Cloud Trace Agent) and the project the Telemetry and Cloud Trace APIs. Without them the API works; only traces are missing.

## Why

- One standard library instead of hand-made correlation; modules need no change, their HTTP, SQL and log calls are captured by instrumentation.
- No extra infrastructure on Cloud Run; cost stays within the Cloud Trace free tier at current traffic and is bounded by one setting.
- A log line, its trace and the Cloud Run request log share one trace id, which is the chain an incident analysis follows: alert → trace → log → code.

## Trade-offs

- Spans add a little CPU and memory and leave the instance in batches; a crash can lose the last few seconds of spans (logs are not affected).
- SQL text is stored in Cloud Trace (without parameter values); treat Cloud Trace access like log access.
- `Host.Jobs` (Cloud Run Jobs) is not traced yet; its logs stay plain text until a later decision.

## Sources and checks

- `src/BuildingBlocks/BuildingBlocks.Observability/`, `src/Bootstrapper/Host.Api/Program.cs`, `tests/UnitTests/BuildingBlocks/Observability/`.
- [Telemetry (OTLP) API](https://docs.cloud.google.com/stackdriver/docs/reference/telemetry/overview), [Cloud Trace IAM](https://docs.cloud.google.com/trace/docs/iam).
- The same decision applies to BreviERP.
