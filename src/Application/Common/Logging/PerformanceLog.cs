namespace Application.Common.Logging;

internal static partial class PerformanceLog
{
    [LoggerMessage(EventId = 1201, Level = LogLevel.Warning,
        Message = "⏳ Slow request {MessageType}: {ElapsedMs} ms (threshold {ThresholdMs} ms)")]
    public static partial void Slow(
        ILogger logger, string messageType, long elapsedMs, int thresholdMs);
}
