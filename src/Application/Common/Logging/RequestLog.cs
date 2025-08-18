namespace Application.Common.Logging;

internal static partial class RequestLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Handling {RequestName}")]
    public static partial void Handling(ILogger logger, string requestName);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information,
        Message = "Handled {RequestName} in {ElapsedMs} ms")]
    public static partial void Handled(ILogger logger, string requestName, long elapsedMs);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Error,
        Message = "Request {RequestName} failed after {ElapsedMs} ms")]
    public static partial void Failed(ILogger logger, string requestName, long elapsedMs, Exception ex);
}
