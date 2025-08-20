namespace Application.Common.Logging;

internal static partial class UnhandledLog
{
    [LoggerMessage(EventId = 1301, Level = LogLevel.Error,
        Message = "⚠️ Unhandled exception in {MessageType}. Exception: {ExceptionType}")]
    public static partial void Error(ILogger logger, string MessageType, string ExceptionType, Exception ex);

    [LoggerMessage(EventId = 1302, Level = LogLevel.Critical,
        Message = "🚨 CRITICAL exception in {MessageType}. Exception: {ExceptionType}")]
    public static partial void Critical(ILogger logger, string MessageType, string ExceptionType, Exception ex);
}
