namespace Application.Common.Logging;

internal static partial class DomainEventLog
{
    [LoggerMessage(EventId = 1401, Level = LogLevel.Information,
        Message = "⚡ Domain events found for {MessageType}: {Count}")]
    public static partial void Found(ILogger logger, string messageType, int count);

    [LoggerMessage(EventId = 1402, Level = LogLevel.Debug,
        Message = "⚡ Dispatching domain event {EventName}")]
    public static partial void Dispatching(ILogger logger, string eventName);

    [LoggerMessage(EventId = 1403, Level = LogLevel.Information,
        Message = "⚡ Dispatched domain event {EventName}")]
    public static partial void Dispatched(ILogger logger, string eventName);

    [LoggerMessage(EventId = 1404, Level = LogLevel.Error,
        Message = "❌ Domain event {EventName} handler failed")]
    public static partial void Failed(ILogger logger, string eventName, Exception ex);
}
