namespace Application.Common.Logging;

internal static partial class CommandLog
{
    [LoggerMessage(EventId = 1501, Level = LogLevel.Information,
        Message = "⚡Catalog import started. File={file}, Size={size}, ProductType={type}")]
    public static partial void ImportCatalogFromXml(
        ILogger logger, string file, long size, int type);
}
