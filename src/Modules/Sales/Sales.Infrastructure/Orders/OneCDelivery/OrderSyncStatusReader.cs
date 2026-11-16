using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.Queries.GetOrderSyncStatus;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Orders;

internal sealed class OrderSyncStatusReader(SalesDbContext dbContext) : IOrderSyncStatusReader
{
    public async Task<GetOrderSyncStatusResult?> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken)
    {
        var id = OrderId.FromStorage(orderId);
        var row = await dbContext.OrdersWithOneCMessage()
            .Where(x => x.Order.Id == id)
            .Select(x => new
            {
                x.Order.OrderNumber,
                x.Order.OneCDocumentNumber,
                x.Order.OneCAcceptedAtUtc,
                x.Order.OneCRejection,
                x.Message
            })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new GetOrderSyncStatusResult(orderId, row.OrderNumber,
                OrderOneCQueries.OneC(row.OneCDocumentNumber, row.OneCRejection, row.Message).Status,
                row.OneCDocumentNumber, row.OneCAcceptedAtUtc);
    }
}
