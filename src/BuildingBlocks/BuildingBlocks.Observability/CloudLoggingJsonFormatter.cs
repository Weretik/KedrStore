using System.Globalization;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;

namespace BuildingBlocks.Observability;

/// <summary>
/// One JSON object per line in the structured format of Google Cloud Logging (spec platform/008, R-005): severity,
/// message, time, the trace and span of the request, the logger, the event properties and the exception for
/// Error Reporting. A property never overwrites one of these fields.
/// </summary>
public sealed class CloudLoggingJsonFormatter(string projectId) : ITextFormatter
{
    private const string TraceField = "logging.googleapis.com/trace";
    private const string SpanField = "logging.googleapis.com/spanId";
    private const string LoggerField = "logger";

    private static readonly HashSet<string> ReservedFields =
        ["severity", "message", "time", TraceField, SpanField, LoggerField, "stack_trace", "SourceContext"];

    private static readonly JsonValueFormatter ValueFormatter = new(typeTagName: null);

    public void Format(LogEvent logEvent, TextWriter output)
    {
        output.Write("{\"severity\":\"");
        output.Write(Severity(logEvent.Level));
        output.Write("\",\"message\":");
        JsonValueFormatter.WriteQuotedJsonString(RenderMessage(logEvent), output);
        output.Write(",\"time\":\"");
        output.Write(logEvent.Timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));
        output.Write('"');

        if (logEvent.TraceId is { } traceId && traceId != default)
        {
            WriteField(TraceField, $"projects/{projectId}/traces/{traceId.ToHexString()}", output);
        }

        if (logEvent.SpanId is { } spanId && spanId != default)
        {
            WriteField(SpanField, spanId.ToHexString(), output);
        }

        if (logEvent.Properties.TryGetValue("SourceContext", out var source) && source is ScalarValue { Value: string logger })
        {
            WriteField(LoggerField, logger, output);
        }

        if (logEvent.Exception is not null)
        {
            WriteField("stack_trace", logEvent.Exception.ToString(), output);
        }

        foreach (var (name, value) in logEvent.Properties)
        {
            if (ReservedFields.Contains(name))
            {
                continue;
            }

            output.Write(',');
            JsonValueFormatter.WriteQuotedJsonString(name, output);
            output.Write(':');
            ValueFormatter.Format(value, output);
        }

        output.Write('}');
        output.WriteLine();
    }

    /// <summary>The message as a person reads it: text values without the quotes Serilog adds by default.</summary>
    private static string RenderMessage(LogEvent logEvent)
    {
        var message = new StringWriter(CultureInfo.InvariantCulture);
        foreach (var token in logEvent.MessageTemplate.Tokens)
        {
            if (token is PropertyToken property
                && logEvent.Properties.TryGetValue(property.PropertyName, out var value)
                && value is ScalarValue { Value: string text })
            {
                message.Write(text);
                continue;
            }

            token.Render(logEvent.Properties, message, CultureInfo.InvariantCulture);
        }

        return message.ToString();
    }

    private static string Severity(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose or LogEventLevel.Debug => "DEBUG",
        LogEventLevel.Information => "INFO",
        LogEventLevel.Warning => "WARNING",
        LogEventLevel.Error => "ERROR",
        _ => "CRITICAL",
    };

    private static void WriteField(string name, string value, TextWriter output)
    {
        output.Write(',');
        JsonValueFormatter.WriteQuotedJsonString(name, output);
        output.Write(':');
        JsonValueFormatter.WriteQuotedJsonString(value, output);
    }
}
