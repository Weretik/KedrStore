namespace BuildingBlocks.Application.Logging;

internal static partial class DomainLog
{
    [LoggerMessage(EventId = 1601, Level = LogLevel.Warning,
        Message = "Domain rule violated in {messageType}. Error: {code}")]
    public static partial void RuleViolated(ILogger logger, string messageType, string code);
}
