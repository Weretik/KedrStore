namespace BuildingBlocks.Application.Logging;

public static partial class CommandLog
{
    [LoggerMessage(EventId = 1701, Level = LogLevel.Information,
        Message = "⚡Catalog import started. File={file}, Size={size}, ProductTypeId={type}")]
    public static partial void ImportCatalogFromXml(
        ILogger logger, string file, long size, int type);
}
