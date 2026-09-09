using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.GetSyncStatus.DTOs;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Orders;

internal sealed class OrderSyncStatusReader(SalesDbContext dbContext) : IOrderSyncStatusReader
{
    public async Task<GetOrderSyncStatusResult?> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken)
    {
        var candidate = await (from order in dbContext.Orders.AsNoTracking()
            join sync in dbContext.OneCOrderSyncs.AsNoTracking() on order.Id equals sync.OrderId
            where order.Id == OrderId.FromStorage(orderId)
            select new { Order = order, Sync = sync })
            .SingleOrDefaultAsync(cancellationToken);

        return candidate is null
            ? null
            : new GetOrderSyncStatusResult(
                candidate.Order.Id.Value,
                candidate.Order.OrderNumber,
                candidate.Sync.Status,
                candidate.Sync.OneCDocumentNumber,
                candidate.Sync.AcceptedAtUtc);
    }
}
