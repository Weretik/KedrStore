using Sales.Domain.QuickOrders;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Notifications;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sales.Infrastructure.QuickOrders;

/// <summary>
/// Sends a stored site order to the managers' chat as an Excel file and records on the order how it went (spec
/// sales/009, R-005). A failure leaves the message for a retry. Runs in the dispatch scope, whose save also stores the
/// order's delivery state.
/// </summary>
internal sealed class QuickOrderTelegramSender(
    ITelegramBotClient bot,
    IOptions<TelegramOptions> options,
    IQuickOrderStore store,
    TimeProvider clock)
    : IOutboxMessageHandler<QuickOrderPlaced>
{
    public async Task<OutboxHandlingResult> HandleAsync(
        QuickOrderPlaced message, Guid messageId, CancellationToken cancellationToken)
    {
        // A message stored before the orders table existed may have no order; it is still sent.
        var order = await store.FindAsync(QuickOrderId.Create(message.OrderId), cancellationToken);
        try
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
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            order?.RecordTelegramFailure(exception.Message, clock.GetUtcNow());
            return OutboxHandlingResult.Failed(exception.Message);
        }

        order?.MarkSentToTelegram(clock.GetUtcNow());
        return OutboxHandlingResult.Delivered;
    }
}
