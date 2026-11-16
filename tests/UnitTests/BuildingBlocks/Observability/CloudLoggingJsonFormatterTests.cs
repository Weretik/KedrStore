using System.Diagnostics;
using System.Text.Json;
using BuildingBlocks.Observability;
using Serilog.Events;
using Serilog.Parsing;

namespace UnitTests.BuildingBlocks.Observability;

public sealed class CloudLoggingJsonFormatterTests
{
    private static readonly ActivityTraceId TraceId = ActivityTraceId.CreateFromString("4bf92f3577b34da6a3ce929d0e0e4736");
    private static readonly ActivitySpanId SpanId = ActivitySpanId.CreateFromString("00f067aa0ba902b7");

    [Theory]
    [InlineData(LogEventLevel.Verbose, "DEBUG")]
    [InlineData(LogEventLevel.Debug, "DEBUG")]
    [InlineData(LogEventLevel.Information, "INFO")]
    [InlineData(LogEventLevel.Warning, "WARNING")]
    [InlineData(LogEventLevel.Error, "ERROR")]
    [InlineData(LogEventLevel.Fatal, "CRITICAL")]
    public void Severity_FollowsTheLevel(LogEventLevel level, string severity)
        => Assert.Equal(severity, Format(Event(level)).GetProperty("severity").GetString());

    [Fact]
    public void Line_IsOneJsonObject_WithTheRenderedMessage()
    {
        var writer = new StringWriter();
        new CloudLoggingJsonFormatter("classkedrproject").Format(Event(LogEventLevel.Information), writer);

        var text = writer.ToString();
        Assert.EndsWith(Environment.NewLine, text);
        Assert.DoesNotContain('\n', text.TrimEnd());
        using var json = JsonDocument.Parse(text);
        Assert.Equal("Order 42 placed", json.RootElement.GetProperty("message").GetString());
        Assert.Equal(42, json.RootElement.GetProperty("OrderId").GetInt32());
        Assert.Equal("Sales.Orders", json.RootElement.GetProperty("logger").GetString());
        Assert.True(json.RootElement.TryGetProperty("time", out _));
    }

    [Fact]
    public void TextValues_AreRenderedWithoutQuotes()
    {
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Now listening on: {Address}, {Count} times"),
            [new LogEventProperty("Address", new ScalarValue("http://[::]:8080")), new LogEventProperty("Count", new ScalarValue(2))]);

        Assert.Equal("Now listening on: http://[::]:8080, 2 times", Format(logEvent).GetProperty("message").GetString());
    }

    [Fact]
    public void TraceAndSpan_UseTheCloudLoggingFields()
    {
        var json = Format(Event(LogEventLevel.Information, traceId: TraceId, spanId: SpanId));

        Assert.Equal("projects/classkedrproject/traces/4bf92f3577b34da6a3ce929d0e0e4736",
            json.GetProperty("logging.googleapis.com/trace").GetString());
        Assert.Equal("00f067aa0ba902b7", json.GetProperty("logging.googleapis.com/spanId").GetString());
    }

    [Fact]
    public void WithoutActivity_NoTraceFields()
    {
        var json = Format(Event(LogEventLevel.Information));

        Assert.False(json.TryGetProperty("logging.googleapis.com/trace", out _));
        Assert.False(json.TryGetProperty("logging.googleapis.com/spanId", out _));
    }

    [Fact]
    public void Exception_GoesToStackTrace()
    {
        var json = Format(Event(LogEventLevel.Error, exception: new InvalidOperationException("boom")));

        var stackTrace = json.GetProperty("stack_trace").GetString();
        Assert.Contains("System.InvalidOperationException: boom", stackTrace);
    }

    [Fact]
    public void Property_NeverOverwritesAReservedField()
    {
        var json = Format(Event(LogEventLevel.Warning, extra: new LogEventProperty("severity", new ScalarValue("INFO"))));

        Assert.Equal("WARNING", json.GetProperty("severity").GetString());
    }

    private static JsonElement Format(LogEvent logEvent)
    {
        var writer = new StringWriter();
        new CloudLoggingJsonFormatter("classkedrproject").Format(logEvent, writer);
        return JsonDocument.Parse(writer.ToString()).RootElement.Clone();
    }

    private static LogEvent Event(
        LogEventLevel level,
        Exception? exception = null,
        ActivityTraceId? traceId = null,
        ActivitySpanId? spanId = null,
        LogEventProperty? extra = null)
    {
        var properties = new List<LogEventProperty>
        {
            new("OrderId", new ScalarValue(42)),
            new("SourceContext", new ScalarValue("Sales.Orders")),
        };
        if (extra is not null)
        {
            properties.Add(extra);
        }

        return new LogEvent(
            DateTimeOffset.Parse("2026-11-16T21:00:00Z"),
            level,
            exception,
            new MessageTemplateParser().Parse("Order {OrderId} placed"),
            properties,
            traceId ?? default,
            spanId ?? default);
    }
}
