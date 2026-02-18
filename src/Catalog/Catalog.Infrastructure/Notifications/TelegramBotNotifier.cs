using Catalog.Application.Features.Orders.Create.Notifications;
using Telegram.Bot.Types;

namespace Catalog.Infrastructure.Notifications;

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

    public async Task SendDocumentAsync(
        string fileName,
        string contentType,
        byte[] bytes,
        string? caption = null,
        CancellationToken cancellationToken = default)
    {
        await using var stream = new MemoryStream(bytes);

        var inputFile = InputFile.FromStream(stream, fileName);

        await _bot.SendDocument(
            chatId: _chatId,
            document: inputFile,
            caption: caption,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }
}
