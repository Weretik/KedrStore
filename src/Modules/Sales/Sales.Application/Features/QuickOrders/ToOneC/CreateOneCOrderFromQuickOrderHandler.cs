using System.Globalization;
using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Domain.Orders;
using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.ToOneC;

/// <summary>
/// Makes an ordinary order for 1C from a site order (spec sales/009, R-010): on the retail counterparty from the
/// settings, with the lines at the prices the buyer saw and a comment naming the buyer. The new order carries its own
/// 1C outbox message, so it goes to 1C like an admin or mobile order. Runs in the dispatch transaction, which saves the
/// order, its message, and the link on the site order together; the link makes a repeated message do nothing.
/// </summary>
public sealed class CreateOneCOrderFromQuickOrderHandler(
    IQuickOrderStore quickOrders,
    IOrderCreationStore orders,
    IOrderNumberGenerator numbers,
    IOptions<SiteOrderOneCOptions> options,
    TimeProvider clock,
    ILogger<CreateOneCOrderFromQuickOrderHandler> logger) : IOutboxMessageHandler<QuickOrderToOneC>
{
    public async Task<OutboxHandlingResult> HandleAsync(
        QuickOrderToOneC message, Guid messageId, CancellationToken cancellationToken)
    {
        var quickOrder = await quickOrders.FindAsync(QuickOrderId.Create(message.QuickOrderId), cancellationToken);
        if (quickOrder is null)
        {
            logger.LogWarning("Quick order {QuickOrderId} is gone; no 1C order is made.", message.QuickOrderId);
            return OutboxHandlingResult.Delivered;
        }

        if (quickOrder.OneCOrderId is not null)
            return OutboxHandlingResult.Delivered;

        var counterpartyId = options.Value.CounterpartyId;
        if (!await orders.CounterpartyExistsAsync(counterpartyId, cancellationToken))
            return OutboxHandlingResult.Failed($"Клієнта {counterpartyId} для замовлень з сайту немає в базі.");

        var now = clock.GetUtcNow();
        var built = Build(quickOrder, counterpartyId, await numbers.GenerateAsync(now, cancellationToken), now);
        if (built.IsFailure)
            return OutboxHandlingResult.Failed(built.Error!.Message);
        var order = built.Value;

        await orders.AddFromOutboxAsync(order, cancellationToken);
        var linked = quickOrder.LinkOneCOrder(order.Id);
        if (linked.IsFailure)
            throw new InvalidOperationException($"Quick order {message.QuickOrderId} got a second 1C order.");

        logger.LogInformation("Quick order {QuickOrderId} became order {OrderNumber} for 1C.",
            message.QuickOrderId, order.OrderNumber);
        return OutboxHandlingResult.Delivered;
    }

    private static DomainResult<Order> Build(QuickOrder quickOrder, string counterpartyId, string number, DateTimeOffset now)
    {
        var lines = new List<OrderLine>(quickOrder.Items.Count);
        foreach (var item in quickOrder.Items)
        {
            if (!int.TryParse(item.ProductId, NumberStyles.None, CultureInfo.InvariantCulture, out var productId))
                return QuickOrderErrors.ProductNotInCatalog(item.ProductId);
            var line = OrderLine.Create(ProductId.FromStorage(productId), item.Title, item.Quantity, item.Amount);
            if (line.IsFailure)
                return line.Error!;
            lines.Add(line.Value);
        }

        return Order.Create(number, counterpartyId, QuickOrderOneCComment.Build(quickOrder), lines, now);
    }
}
