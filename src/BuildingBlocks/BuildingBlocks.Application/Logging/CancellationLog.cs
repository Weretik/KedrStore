namespace BuildingBlocks.Application.Logging;

internal static partial class CancellationLog
{
    [LoggerMessage(EventId = 1501, Level = LogLevel.Warning,
        Message = "⏹️ Operation cancelled in {messageType}")]
    public static partial void Cancelled(ILogger logger, string messageType);
}
