# Request tracing and log correlation

**Static code check:** 2026-11-16. **Owner:** platform ([`BuildingBlocks.Observability`](../../../../src/BuildingBlocks/BuildingBlocks.Observability/), wired in `Host.Api`).

In production every API request becomes a trace in Google Cloud Trace (server span, SQL commands, outgoing HTTP calls, exceptions), and the console log is Cloud Logging JSON whose lines carry the trace id. Cloud Logging opens the trace from a log line, and Trace Explorer shows the logs of a trace. Without `Observability:GoogleCloudProjectId` (development, tests) nothing is exported and logs stay plain text.

## Structure and relationships

- [ObservabilityRegistrationExtensions](../../../../src/BuildingBlocks/BuildingBlocks.Observability/ObservabilityRegistrationExtensions.cs) — `builder.AddObservability()` in [Program.cs](../../../../src/Bootstrapper/Host.Api/Program.cs): Serilog from the `Serilog` section, console sink (JSON or text), then tracing.
- [CloudTracing](../../../../src/BuildingBlocks/BuildingBlocks.Observability/CloudTracing.cs) — OpenTelemetry resource (`service.name`, `gcp.project_id`), ratio sampler by trace id, ASP.NET Core (no `/health`), HttpClient and Npgsql instrumentation, OTLP/HTTP exporter to `telemetry.googleapis.com`.
- [GoogleCloudAuthHandler](../../../../src/BuildingBlocks/BuildingBlocks.Observability/GoogleCloudAuthHandler.cs) — bearer token of the Cloud Run runtime account for the exporter; on Cloud Run the metadata server's credential is built directly, because the Application Default Credentials ping runs on the throttled export thread and caches a miss for the whole process.
- [CloudLoggingJsonFormatter](../../../../src/BuildingBlocks/BuildingBlocks.Observability/CloudLoggingJsonFormatter.cs) — `severity`, `message`, `time`, `logging.googleapis.com/trace`, `logging.googleapis.com/spanId`, `logger`, properties, `stack_trace`.
- Settings: `Observability` in [appsettings.json](../../../../src/Bootstrapper/Host.Api/appsettings.json) (`ServiceName` `kedr-api`, `TraceSampleRatio`), project id in [appsettings.Production.json](../../../../src/Bootstrapper/Host.Api/appsettings.Production.json).

## Code structure

```text
src/BuildingBlocks/BuildingBlocks.Observability/
├── ObservabilityOptions.cs
├── ObservabilityRegistrationExtensions.cs
├── CloudTracing.cs
├── GoogleCloudAuthHandler.cs
└── CloudLoggingJsonFormatter.cs
```

## Tests

- Unit: [formatter](../../../../tests/UnitTests/BuildingBlocks/Observability/CloudLoggingJsonFormatterTests.cs), [tracing registration and sampler](../../../../tests/UnitTests/BuildingBlocks/Observability/CloudTracingTests.cs), [auth handler](../../../../tests/UnitTests/BuildingBlocks/Observability/GoogleCloudAuthHandlerTests.cs) — `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Observability"`.

## Related sources

- [Specification](../../specs/platform/008-request-tracing/README.md), [ADR-0014](../../architecture/decisions/0014-opentelemetry-traces-to-cloud-trace.md)
- [How to open a trace and its logs](../../operations/diagnostics/health-logs-swagger.md#traces-in-google-cloud)
