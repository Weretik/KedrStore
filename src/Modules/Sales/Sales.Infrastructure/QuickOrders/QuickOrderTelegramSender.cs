using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Notifications;
using Sales.Application.Features.QuickOrders.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sales.Infrastructure.QuickOrders;

/// <summary>Sends a stored quick order to the managers' chat as an Excel file. A failure leaves it for a retry.</summary>
internal sealed class QuickOrderTelegramSender(ITelegramBotClient bot, IOptions<TelegramOptions> options)
    : IOutboxMessageHandler<QuickOrderPlaced>
{
    public async Task HandleAsync(QuickOrderPlaced message, Guid messageId, CancellationToken cancellationToken)
    {
        var bytes = QuickOrderExcelFile.Build(message);
        await using var stream = new MemoryStream(bytes);

        await bot.SendDocument(
            chatId: options.Value.ChatId,
            document: InputFile.FromStream(stream, QuickOrderExcelFile.NameOf(message)),
            caption: QuickOrderTelegramCaption.Build(message),
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }
}
