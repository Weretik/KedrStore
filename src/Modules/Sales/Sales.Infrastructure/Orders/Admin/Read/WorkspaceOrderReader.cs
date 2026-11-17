using BuildingBlocks.Application.Text;
using Sales.Application.Abstractions.Workspace;
using static Sales.Infrastructure.Orders.WorkspaceQueryTools;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Orders;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Orders;

internal sealed class WorkspaceOrderReader(SalesDbContext db) : IWorkspaceOrderReader
{
    public async Task<WorkspacePage<WorkspaceOrderListItem>> GetOrdersAsync(
        string? searchTerm, string? counterpartyId,
        DateTimeOffset? createdFromUtc, DateTimeOffset? createdToUtcExclusive,
        string sort, int page, int pageSize, CancellationToken ct)
    {
        var query =
            from row in db.OrdersWithOneCMessage()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking() on row.Order.CounterpartyId equals customer.Id
            where counterpartyId == null || row.Order.CounterpartyId == CounterpartyId.FromStorage(counterpartyId)
            select new { order = row.Order, customer, message = row.Message };
        if (createdFromUtc is { } fromUtc)
            query = query.Where(x => x.order.CreatedAt >= fromUtc);
        if (createdToUtcExclusive is { } toUtc)
            query = query.Where(x => x.order.CreatedAt < toUtc);
        // Database rules 2.1: every word must match; trigram indexes serve ILIKE on these columns.
        foreach (var term in Terms(searchTerm))
        {
            var pattern = "%" + SearchWords.Escape(term) + "%";
            var numericId = long.TryParse(term, out var parsedId) ? parsedId : -1;
            query = query.Where(x => EF.Functions.ILike(x.order.OrderNumber, pattern, SearchWords.EscapeCharacter) ||
                                     EF.Functions.ILike(x.customer.Name, pattern, SearchWords.EscapeCharacter) ||
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
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, "The validator allows only known sorts.")
        };
        var rows = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new
            {
                x.order.Id, x.order.OrderNumber, x.order.CounterpartyId, CustomerName = x.customer.Name,
                x.order.CreatedAt, LineCount = x.order.Lines.Count, Total = x.order.Lines.Sum(y => y.Amount),
                Adjustment = x.order.Lines.Sum(y => y.PriceAdjustment * y.Quantity),
                x.order.OneCDocumentNumber, x.order.OneCRejection, x.message
            })
            .ToListAsync(ct);
        var items = rows.Select(x =>
        {
            var oneC = OrderOneCQueries.OneC(x.OneCDocumentNumber, x.OneCRejection, x.message);
            return new WorkspaceOrderListItem(x.Id.Value, x.OrderNumber, x.CounterpartyId.Value, x.CustomerName,
                x.CreatedAt, x.LineCount, x.Total, oneC.Status.ToString(), x.Adjustment,
                oneC.Error, oneC.Attempts, oneC.LastAttemptAtUtc, oneC.NextAttemptAtUtc);
        }).ToList();
        return new WorkspacePage<WorkspaceOrderListItem>(PageInfo(page, pageSize, count), items);
    }

    public async Task<WorkspaceOrderDetail?> GetOrderAsync(long orderId, CancellationToken ct)
    {
        if (orderId < 1) return null;
        var id = OrderId.FromStorage(orderId);
        var row = await (
            from x in db.OrdersWithOneCMessage()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking() on x.Order.CounterpartyId equals customer.Id
            where x.Order.Id == id
            select new { order = x.Order, customer, message = x.Message }).SingleOrDefaultAsync(ct);
        if (row is null) return null;
        var stored = await db.OrderLines.AsNoTracking()
            .Where(x => EF.Property<OrderId>(x, "OrderId") == id)
            .OrderBy(x => x.Id).Select(x => new WorkspaceStoredLine(x.ProductId, x.ProductName, x.Quantity, x.Amount, x.PriceAdjustment))
            .ToArrayAsync(ct);
        var oneC = OrderOneCQueries.OneC(row.order.OneCDocumentNumber, row.order.OneCRejection, row.message);
        return new WorkspaceOrderDetail(orderId, row.order.OrderNumber, row.order.CounterpartyId.Value,
            row.customer.Name, row.order.CreatedAt, row.order.Comment, stored,
            stored.Sum(x => x.Amount), oneC.Status.ToString(), row.order.OneCDocumentNumber,
            oneC.Error, oneC.Attempts, oneC.LastAttemptAtUtc, oneC.NextAttemptAtUtc);
    }
}
