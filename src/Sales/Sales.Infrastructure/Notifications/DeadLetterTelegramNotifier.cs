using BuildingBlocks.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Sales.Application.Features.Orders.DeliveryFailure;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sales.Infrastructure.Notifications;

public sealed class DeadLetterTelegramNotifier(
    ITelegramBotClient bot,
    IOptions<TelegramOptions> options) : IDeadLetterNotifier
{
    public async Task SendAsync(
        DeadLetterNotification notification,
        DeadLetterExcelFile attachment,
        CancellationToken cancellationToken = default)
    {
        var caption = DeadLetterTelegramCaption.Build(notification);
        await using var stream = new MemoryStream(attachment.Bytes);

        await bot.SendDocument(
            chatId: options.Value.ChatId,
            document: InputFile.FromStream(stream, attachment.FileName),
            caption: caption,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }
}
