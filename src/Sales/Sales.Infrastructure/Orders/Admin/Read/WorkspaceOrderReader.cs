using static Sales.Infrastructure.Orders.WorkspaceQueryTools;
using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed class WorkspaceOrderReader(SalesDbContext db) : IWorkspaceOrderReader
{
    public async Task<WorkspacePage<WorkspaceOrderListItem>> GetOrdersAsync(
        string? searchTerm, string? counterpartyId,
        DateTimeOffset? createdFromUtc, DateTimeOffset? createdToUtcExclusive,
        string sort, int page, int pageSize, CancellationToken ct)
    {
        ValidatePage(searchTerm, counterpartyId, page, pageSize);
        var query =
            from order in db.Orders.AsNoTracking()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking() on order.CounterpartyId equals customer.Id
            join sync in db.OneCOrderSyncs.AsNoTracking() on order.Id equals sync.OrderId
            where counterpartyId == null || order.CounterpartyId == counterpartyId
            select new { order, customer, sync };
        if (createdFromUtc is { } fromUtc)
            query = query.Where(x => x.order.CreatedAt >= fromUtc);
        if (createdToUtcExclusive is { } toUtc)
            query = query.Where(x => x.order.CreatedAt < toUtc);
        foreach (var term in Terms(searchTerm))
        {
            var pattern = "%" + EscapeLike(term) + "%";
            var numericId = long.TryParse(term, out var parsedId) ? parsedId : -1;
            query = query.Where(x => EF.Functions.ILike(x.order.OrderNumber, pattern, "\\") ||
                                     EF.Functions.ILike(x.customer.Name, pattern, "\\") ||
                                     x.order.Id == OrderId.FromStorage(numericId));
        }
        var count = await query.LongCountAsync(ct);
        query = sort switch
        {
            "createdAt" => query.OrderBy(x => x.order.CreatedAt).ThenBy(x => x.order.Id),
            "-createdAt" => query.OrderByDescending(x => x.order.CreatedAt).ThenByDescending(x => x.order.Id),
            "orderNumber" => query.OrderBy(x => x.order.OrderNumber).ThenBy(x => x.order.Id),
            "-orderNumber" => query.OrderByDescending(x => x.order.OrderNumber).ThenByDescending(x => x.order.Id),
            "customerName" => query.OrderBy(x => x.customer.Name).ThenBy(x => x.order.Id),
            "-customerName" => query.OrderByDescending(x => x.customer.Name).ThenByDescending(x => x.order.Id),
            "totalAmount" => query.OrderBy(x => x.order.Lines.Sum(y => y.Amount)).ThenBy(x => x.order.Id),
            "-totalAmount" => query.OrderByDescending(x => x.order.Lines.Sum(y => y.Amount)).ThenByDescending(x => x.order.Id),
            _ => throw new WorkspaceException(400, "invalid_sort", "Unsupported order sort.")
        };
        var rows = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new WorkspaceOrderListItem(x.order.Id.Value, x.order.OrderNumber,
                x.order.CounterpartyId, x.customer.Name, x.order.CreatedAt,
                x.order.Lines.Count, x.order.Lines.Sum(y => y.Amount), x.sync.Status.ToString(),
                x.order.Lines.Sum(y => y.PriceAdjustment * y.Quantity)))
            .ToListAsync(ct);
        return new WorkspacePage<WorkspaceOrderListItem>(PageInfo(page, pageSize, count), rows);
    }

    public async Task<WorkspaceOrderDetail?> GetOrderAsync(long orderId, CancellationToken ct)
    {
        if (orderId < 1) return null;
        var id = OrderId.FromStorage(orderId);
        var row = await (
            from order in db.Orders.AsNoTracking()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking() on order.CounterpartyId equals customer.Id
            join sync in db.OneCOrderSyncs.AsNoTracking() on order.Id equals sync.OrderId
            where order.Id == id
            select new { order, customer, sync }).SingleOrDefaultAsync(ct);
        if (row is null) return null;
        var stored = await db.OrderLines.AsNoTracking()
            .Where(x => EF.Property<OrderId>(x, "OrderId") == id)
            .OrderBy(x => x.Id).Select(x => new WorkspaceStoredLine(x.ProductId, x.ProductName, x.Quantity, x.Amount, x.PriceAdjustment))
            .ToArrayAsync(ct);
        return new WorkspaceOrderDetail(orderId, row.order.OrderNumber, row.order.CounterpartyId,
            row.customer.Name, row.order.CreatedAt, row.order.Comment, stored,
            stored.Sum(x => x.Amount), row.sync.Status.ToString(), row.sync.OneCDocumentNumber);
    }
}
