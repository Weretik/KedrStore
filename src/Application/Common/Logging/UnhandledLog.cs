namespace Application.Common.Logging;

internal static partial class UnhandledLog
{
    [LoggerMessage(EventId = 1301, Level = LogLevel.Error,
        Message = "⚠️ Unhandled exception in {messageType}. Exception: {exceptionType}")]
    public static partial void Error(ILogger logger, string messageType, string exceptionType, Exception ex);

    [LoggerMessage(EventId = 1302, Level = LogLevel.Critical,
        Message = "🚨 CRITICAL exception in {messageType}. Exception: {exceptionType}")]
    public static partial void Critical(ILogger logger, string messageType, string exceptionType, Exception ex);
}
