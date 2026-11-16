# Platform 008: Request tracing and log correlation

**Status:** accepted 2026-11-16 (owner: «если нет, нужно в проекты ввести трейсы»; plan approved the same day). **Owner:** platform (shared code in `BuildingBlocks.Observability`, composition in `Host.Api`). **Decision:** [ADR-0014](../../../architecture/decisions/0014-opentelemetry-traces-to-cloud-trace.md).

## Goal

When production breaks, the path from a symptom to the line of code is short: a request that failed is a **trace** in Google Cloud Trace (which endpoint, how long each database query and each outgoing call took, where the exception was thrown), and every log line written during that request carries the same trace id, so Cloud Logging opens the trace from the log and the logs from the trace.

**Today (checked 2026-11-16):** no tracing at all (no OpenTelemetry, no `ActivitySource` export). Serilog writes plain text to the console, so Cloud Logging stores every line as `textPayload` without a severity: an error is not distinguishable from an information line, and nothing links a line to its request.

**Out of scope:** alerts in Cloud Monitoring, a read-only account for incident analysis, an incident-analysis skill, metrics, traces of `Host.Jobs` (Cloud Run Jobs). Each is a separate decision of the owner.

## Rules

- R-001 `Host.Api` exports traces with OpenTelemetry: incoming HTTP requests (ASP.NET Core), outgoing HTTP calls (1C SOAP, Telegram, CDN, Google APIs) and PostgreSQL commands (Npgsql). Exceptions are recorded on the span. `/health` is not traced.
- R-002 Traces go straight to Google Cloud Trace through the Telemetry (OTLP) API `https://telemetry.googleapis.com/v1/traces` (OTLP over HTTP/protobuf), authenticated with the Cloud Run runtime account (Application Default Credentials). No collector, sidecar or agent.
- R-003 Export is on only when `Observability:GoogleCloudProjectId` is set (production: `appsettings.Production.json`). Development, tests and the build-time OpenAPI run export nothing and keep plain-text console logs.
- R-004 `Observability:TraceSampleRatio` (0…1, default 1) is the share of traces kept, decided by trace id. The sampler ignores the sampled flag of the incoming `traceparent`, because the Cloud Run front end marks most requests as not sampled; the trace id of the incoming header is kept, so the Cloud Run request log and our spans join one trace.
- R-005 With a project id, the console log is one JSON object per line in the Cloud Logging format: `severity`, `message`, `time`, `logging.googleapis.com/trace` (`projects/<id>/traces/<trace id>`), `logging.googleapis.com/spanId`, `logger`, the event properties, and `stack_trace` with the exception text (Error Reporting reads it). A property never overwrites one of these fields.
- R-006 A failed export never fails a request or the startup: without the Cloud Trace permission the API works as before and only the traces are missing.
- R-007 Nothing secret is added to spans: SQL text is recorded without parameter values; request bodies, cookies and headers are not recorded; the query string keeps the instrumentation's default redaction.

## Scenarios

- SC-001 Production request `GET /api/catalog/...`: one trace with the server span and its SQL spans; every log line of that request shows a link to it in Cloud Logging.
- SC-002 An endpoint throws: the trace has the exception event; the log line is `severity: ERROR` with `stack_trace`, and Error Reporting groups it.
- SC-003 Development or an integration test: no export attempt, log lines are plain text as before.
- SC-004 The runtime account has no Cloud Trace permission: requests answer as before; the export errors stay inside OpenTelemetry.
- SC-005 `TraceSampleRatio` 0.25: about a quarter of traces are exported, whatever the incoming `traceparent` says.
- SC-006 `GET /health`: no span.

## Design

```text
src/BuildingBlocks/BuildingBlocks.Observability/     new project, referenced only by hosts
├── ObservabilityOptions.cs                section "Observability": ServiceName, GoogleCloudProjectId, TraceSampleRatio
├── ObservabilityRegistrationExtensions.cs builder.AddObservability(): Serilog (text or Cloud Logging JSON) + tracing
├── CloudTracing.cs                        OpenTelemetry: resource, sampler, instrumentation, OTLP exporter
├── GoogleCloudAuthHandler.cs              DelegatingHandler: bearer token of the runtime account for the exporter
└── CloudLoggingJsonFormatter.cs           Serilog ITextFormatter: R-005
src/Bootstrapper/Host.Api/Program.cs       builder.AddObservability() instead of AddSerilog() (SerilogExtensions.cs removed)
```

Packages (central versions): `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `Npgsql.OpenTelemetry`; `Google.Apis.Auth` and Serilog already exist.

**Google Cloud (owner, once per project `classkedrproject`):** enable the Telemetry API and the Cloud Trace API; give the Cloud Run runtime account the role Cloud Trace Agent (`roles/cloudtrace.agent`, holds `telemetry.traces.write`). The steps are in [diagnostics](../../../operations/diagnostics/health-logs-swagger.md#traces-in-google-cloud).

## Tasks

| ID | Task | Status |
| --- | --- | --- |
| TS-001 | Tests: JSON formatter (severity, trace, stack trace, reserved fields), auth handler, registration on/off | done |
| TS-002 | `BuildingBlocks.Observability` and `Host.Api` wiring | done |
| TS-003 | ADR-0014, C4, diagnostics runbook, feature map | done |
| EN-001 | Owner enables the APIs and the role in Google Cloud | waiting for the owner |

## Verification

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Observability"`
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`
- After deploy: Cloud Logging shows JSON lines with severity for `kedr-api`, and Trace Explorer shows traces of service `kedr-api`.
