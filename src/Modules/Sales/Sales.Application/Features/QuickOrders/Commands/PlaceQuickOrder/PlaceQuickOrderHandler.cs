using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Domain.QuickOrders;

namespace Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;

/// <summary>
/// Stores the site order with its Telegram and 1C outbox messages, then tries both once (spec sales/009, R-001…R-003, R-010;
/// platform spec 006, R-003). The buyer gets the order id even when Telegram is down. With an idempotency key the key
/// is the order id: a repeat answers with the stored order and sends nothing again; the same key with other data is a
/// conflict.
/// </summary>
public sealed class PlaceQuickOrderHandler(
    IQuickOrderStore store,
    IOutboxDispatcher outbox,
    TimeProvider clock,
    ILogger<PlaceQuickOrderHandler> logger)
    : ICommandHandler<PlaceQuickOrderCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(PlaceQuickOrderCommand command, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var id = command.IdempotencyKey is { } key ? QuickOrderId.Create(key) : QuickOrderId.New(now);
        var created = Build(id, command, now);
        if (created.IsFailure)
            return created.Error!.ToResult<Guid>();
        var order = created.Value;

        // R-002: the input is valid, so a buyer, the site or the proxy that stops waiting must not cancel a half-made
        // order. From here the work runs to the end; only the answer may be lost.
        if (command.IdempotencyKey is not null && await store.FindAsync(id, CancellationToken.None) is { } stored)
            return Repeat(stored, order);

        var toOneC = new QuickOrderToOneC(id.Value);
        if (!await store.TryAddAsync(order, QuickOrderPlaced.From(order), toOneC, CancellationToken.None))
        {
            // Another copy of the same request was stored between the check and the save: answer as it did.
            var winner = await store.FindAsync(id, CancellationToken.None)
                ?? throw new InvalidOperationException($"Quick order {id.Value} was neither saved nor found.");
            return Repeat(winner, order);
        }

        logger.LogInformation("Quick order {QuickOrderId} placed with {LineCount} lines, total {Total}.",
            id.Value, order.Items.Count, order.Total);
        // R-010: the 1C order is made right away too; it reaches 1C through its own message in the 1C lane.
        await outbox.DispatchAsync([id.Value, order.OneCOrderMessageId!.Value], CancellationToken.None);

        return Result.Success(id.Value);
    }

    private static DomainResult<QuickOrder> Build(QuickOrderId id, PlaceQuickOrderCommand command, DateTimeOffset now)
    {
        var items = new List<QuickOrderItem>(command.Lines.Count);
        foreach (var line in command.Lines)
        {
            var item = QuickOrderItem.Create(line.ProductId, line.Title, line.UnitPrice, line.Quantity);
            if (item.IsFailure)
                return item.Error!;
            items.Add(item.Value);
        }

        return QuickOrder.Create(id, command.FirstName, command.Phone, items, now, command.Comment);
    }

    private static Result<Guid> Repeat(QuickOrder stored, QuickOrder candidate) =>
        stored.SameAs(candidate)
            ? Result.Success(stored.Id.Value)
            : QuickOrderErrors.IdempotencyConflict().ToResult<Guid>();
}
