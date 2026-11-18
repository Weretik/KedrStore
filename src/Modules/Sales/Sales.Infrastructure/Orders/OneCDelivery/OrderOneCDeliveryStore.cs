using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.OneCDelivery.Messages;
using BuildingBlocks.Infrastructure.Outbox.Persistence;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Orders;

/// <summary>
/// Data of the 1C delivery handlers. It shares the dispatch's <see cref="SalesDbContext"/>, so the order it loads and
/// the signal it adds are saved with the message outcome (platform spec 006, R-008).
/// </summary>
internal sealed class OrderOneCDeliveryStore(SalesDbContext db, OutboxWriter outbox) : IOrderOneCDeliveryStore
{
    public Task<Order?> FindForDeliveryAsync(long orderId, CancellationToken cancellationToken)
        => db.Orders.Include(order => order.Lines)
            .SingleOrDefaultAsync(order => order.Id == OrderId.FromStorage(orderId), cancellationToken);

    public void AddDeliveryFailedSignal(OrderOneCDeliveryFailed signal) => outbox.Add(db, signal);

    public async Task<DeadLetterNotification?> BuildFailureNotificationAsync(
        OrderOneCDeliveryFailed signal,
        CancellationToken cancellationToken)
    {
        var id = OrderId.FromStorage(signal.OrderId);
        var row = await (
                from order in db.Orders.AsNoTracking()
                join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking()
                    on order.CounterpartyId equals customer.Id
                where order.Id == id
                select new
                {
                    order.OrderNumber,
                    customer.Id,
                    customer.Name,
                    Lines = order.Lines.OrderBy(line => line.Id)
                        .Select(line => new DeadLetterOrderLine(line.ProductId.ToString(), line.ProductName, line.Quantity, line.Amount))
                        .ToList()
                })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new DeadLetterNotification(signal.OrderId, row.OrderNumber, row.Id.Value, row.Name, signal.LastError,
                signal.Attempts, signal.FailedAtUtc, row.Lines);
    }
}
