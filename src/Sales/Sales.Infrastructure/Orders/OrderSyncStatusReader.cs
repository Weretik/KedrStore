using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.GetSyncStatus.DTOs;

namespace Sales.Infrastructure.Orders;

internal sealed class OrderSyncStatusReader(SalesDbContext dbContext) : IOrderSyncStatusReader
{
    public Task<GetOrderSyncStatusResult?> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken)
        => (from order in dbContext.Orders.AsNoTracking()
            join sync in dbContext.OneCOrderSyncs.AsNoTracking() on order.Id equals sync.OrderId
            where order.Id.Value == orderId
            select new GetOrderSyncStatusResult(
                order.Id.Value,
                order.OrderNumber,
                sync.Status,
                sync.OneCDocumentNumber,
                sync.AcceptedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
}
