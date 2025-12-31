namespace BuildingBlocks.Application.Logging;

internal static partial class RequestLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "ℹ️ Handling {requestName}")]
    public static partial void Handling(ILogger logger, string requestName);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information,
        Message = "ℹ️ Handled {requestName} in {elapsedMs} ms")]
    public static partial void Handled(ILogger logger, string requestName, long elapsedMs);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Error,
        Message = "❌ Request {requestName} failed after {elapsedMs} ms")]
    public static partial void Failed(ILogger logger, string requestName, long elapsedMs, Exception ex);
}
