using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Observability;

/// <summary>
/// OpenTelemetry traces of the host, exported straight to Google Cloud Trace through the Telemetry (OTLP) API
/// (spec platform/008, ADR-0014). Registered only when a Google Cloud project is configured.
/// </summary>
public static class CloudTracing
{
    private static readonly Uri TelemetryApiTraces = new("https://telemetry.googleapis.com/v1/traces");

    public static void Add(IServiceCollection services, ObservabilityOptions options)
    {
        if (!options.ExportsToGoogleCloud)
        {
            return;
        }

        var sampler = CreateSampler(options.TraceSampleRatio);
        services.AddHostedService<OpenTelemetryErrorLog>();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(options.ServiceName)
                .AddAttributes([new("gcp.project_id", options.GoogleCloudProjectId!)]))
            .WithTracing(tracing => tracing
                .SetSampler(sampler)
                .AddAspNetCoreInstrumentation(instrumentation =>
                {
                    instrumentation.Filter = ShouldTrace;
                    instrumentation.RecordException = true;
                })
                .AddHttpClientInstrumentation(instrumentation => instrumentation.RecordException = true)
                .AddNpgsql()
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = TelemetryApiTraces;
                    exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                    exporter.HttpClientFactory = () => new HttpClient(
                        GoogleCloudAuthHandler.FromRuntimeCredentials()
                            .WithInner(new SocketsHttpHandler()));
                }));
    }

    /// <summary>
    /// Keeps a share of traces by trace id and ignores the incoming sampled flag: the Cloud Run front end marks most
    /// requests as not sampled, yet its trace id is kept so its request log joins our trace (R-004).
    /// </summary>
    public static Sampler CreateSampler(double ratio)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ratio, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ratio, 1);
        return new TraceIdRatioBasedSampler(ratio);
    }

    public static bool ShouldTrace(HttpContext context)
        => !context.Request.Path.StartsWithSegments("/health");

    private static DelegatingHandler WithInner(this DelegatingHandler handler, HttpMessageHandler inner)
    {
        handler.InnerHandler = inner;
        return handler;
    }
}
