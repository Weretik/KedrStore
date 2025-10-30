

using Application.Catalog.CreateQuickOrder;

namespace Infrastructure.Catalog.Notifications;

public sealed class TelegramBotNotifier(ITelegramBotClient bot, IOptions<TelegramOptions> options)
    : ITelegramNotifier
{
    private readonly ITelegramBotClient _bot = bot;
    private readonly string _chatId = options.Value.ChatId;
    public Task SendMessageAsync(string text, CancellationToken cancellationToken = default)
        => _bot.SendMessage(
            chatId: _chatId,
            text: text,
            ParseMode.Html,
            cancellationToken: cancellationToken);
}
