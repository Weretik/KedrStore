namespace BuildingBlocks.Infrastructure.Services;

public sealed class TelegramOptions
{
    public string BotToken { get; init; } = string.Empty;
    public string ChatId   { get; init; } = string.Empty;
}
