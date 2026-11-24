using Sales.Domain.QuickOrders;
using BuildingBlocks.Application.Outbox;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Application.Features.QuickOrders.Messages;

/// <summary>
/// The last attempt to send a site order to Telegram failed: the order shows «Не надіслано» in the Admin and waits for
/// a person (spec sales/009, R-005). No Telegram alert: the same broken chat would swallow it. Runs in the dispatch
/// transaction, which saves the change.
/// </summary>
public sealed class QuickOrderTelegramGiveUpHandler(IQuickOrderStore store, TimeProvider clock)
    : IOutboxGiveUpHandler<QuickOrderPlaced>
{
    public async Task HandleGiveUpAsync(QuickOrderPlaced message, OutboxGiveUp giveUp, CancellationToken cancellationToken)
    {
        var order = await store.FindAsync(QuickOrderId.Create(message.OrderId), cancellationToken);
        order?.GiveUpTelegram(giveUp.LastError, clock.GetUtcNow());
    }
}
