using System.Diagnostics.Tracing;
using BuildingBlocks.Observability;
using Microsoft.Extensions.Logging;

namespace UnitTests.BuildingBlocks.Observability;

public sealed class OpenTelemetryErrorLogTests
{
    [Fact]
    public void SdkError_IsWrittenToTheApplicationLog()
    {
        var logger = new RecordingLogger();
        using var errorLog = new OpenTelemetryErrorLog(logger);
        using var source = new FakeExporterEventSource();

        source.ExportFailed("https://telemetry.googleapis.com/v1/traces", "403 Forbidden");

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("403 Forbidden", entry.Message);
        Assert.Contains("telemetry.googleapis.com", entry.Message);
    }

    [Fact]
    public void SdkInformation_IsNotWritten()
    {
        var logger = new RecordingLogger();
        using var errorLog = new OpenTelemetryErrorLog(logger);
        using var source = new FakeExporterEventSource();

        source.Exported(12);

        Assert.Empty(logger.Entries);
    }

    [EventSource(Name = "OpenTelemetry-Exporter-Fake")]
    private sealed class FakeExporterEventSource : EventSource
    {
        [Event(1, Level = EventLevel.Error, Message = "Export to '{0}' failed: {1}")]
        public void ExportFailed(string endpoint, string error) => WriteEvent(1, endpoint, error);

        [Event(2, Level = EventLevel.Informational, Message = "Exported {0} spans")]
        public void Exported(int count) => WriteEvent(2, count);
    }

    private sealed class RecordingLogger : ILogger<OpenTelemetryErrorLog>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }
}
