namespace Infrastructure.Catalog.Notifications;

public sealed class TelegramBotNotifier(IOptions<TelegramOptions> options) : ITelegramNotifier
{
    private readonly ITelegramBotClient _bot = new TelegramBotClient(options.Value.BotToken);
    private readonly string _chatId = options.Value.ChatId;

    public Task SendMessageAsync(string text, CancellationToken cancellationToken = default)
        => _bot.SendMessage(
            chatId: _chatId,
            text: text,
            cancellationToken: cancellationToken);
}
