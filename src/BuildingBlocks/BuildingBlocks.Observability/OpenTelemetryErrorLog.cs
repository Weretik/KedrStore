using System.Diagnostics.Tracing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Observability;

/// <summary>
/// Writes the OpenTelemetry SDK's own warnings and errors (a failed export to the Telemetry API, a bad credential)
/// to the application log. The SDK reports them only through <see cref="EventSource"/>, so without this listener a
/// broken export is silent on Cloud Run (spec platform/008).
/// </summary>
public sealed class OpenTelemetryErrorLog(ILogger<OpenTelemetryErrorLog> logger) : EventListener, IHostedService
{
    private const string SourcePrefix = "OpenTelemetry";

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        // Runs from the base constructor too, before the logger is assigned; enabling a source needs no state.
        if (eventSource.Name.StartsWith(SourcePrefix, StringComparison.Ordinal))
        {
            EnableEvents(eventSource, EventLevel.Warning, EventKeywords.All);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (logger is null || eventData.Level > EventLevel.Warning)
        {
            return;
        }

        var level = eventData.Level is EventLevel.Critical or EventLevel.Error ? LogLevel.Error : LogLevel.Warning;
        logger.Log(level, "OpenTelemetry {EventSource}: {Event}", eventData.EventSource.Name, Describe(eventData));
    }

    public static string Describe(EventWrittenEventArgs eventData)
    {
        var payload = eventData.Payload?.ToArray() ?? [];
        if (string.IsNullOrEmpty(eventData.Message))
        {
            return $"{eventData.EventName} {string.Join(", ", payload)}";
        }

        try
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, eventData.Message, payload);
        }
        catch (FormatException)
        {
            return $"{eventData.Message} {string.Join(", ", payload)}";
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
