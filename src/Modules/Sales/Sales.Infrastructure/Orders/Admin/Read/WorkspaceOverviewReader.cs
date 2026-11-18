using Sales.Application.Abstractions.Orders;
using Sales.Domain.Orders;
using Sales.Application.Abstractions.Workspace.Overview;
using Sales.Application.Abstractions.Workspace.Orders;

namespace Sales.Infrastructure.Orders;

/// <summary>Counts and short lists for the admin «Огляд»; a sync problem is a status the manual retry accepts.</summary>
internal sealed class WorkspaceOverviewReader(SalesDbContext db) : IWorkspaceOverviewReader
{
    public async Task<WorkspaceOverview> GetAsync(DateTimeOffset todayFromUtc, CancellationToken ct)
    {
        var orders =
            from row in db.OrdersWithOneCMessage()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking() on row.Order.CounterpartyId equals customer.Id
            select new { row, customer };
        var problems = db.OrdersWithOneCMessage().Where(OrderOneCQueries.IsProblem)
            .Join(db.Counterparties.IgnoreQueryFilters().AsNoTracking(), row => row.Order.CounterpartyId,
                customer => customer.Id, (row, customer) => new { row, customer });

        var ordersToday = await db.Orders.AsNoTracking().CountAsync(order => order.CreatedAt >= todayFromUtc, ct);
        var draftCount = await db.OrderDrafts.AsNoTracking().CountAsync(ct);
        var problemCount = await problems.CountAsync(ct);
        var problemRows = (await problems
                .OrderByDescending(x => x.row.Order.CreatedAt).ThenByDescending(x => x.row.Order.Id)
                .Take(WorkspaceOverview.ListSize)
                .Select(x => new
                {
                    x.row.Order.Id, x.row.Order.OrderNumber, x.customer.Name, x.row.Order.CreatedAt,
                    x.row.Order.OneCDocumentNumber, x.row.Order.OneCRejection, x.row.Message
                })
                .ToListAsync(ct))
            .Select(x =>
            {
                var oneC = OrderOneCQueries.OneC(x.OneCDocumentNumber, x.OneCRejection, x.Message);
                return new WorkspaceSyncProblem(x.Id.Value, x.OrderNumber, x.Name, x.CreatedAt,
                    oneC.Status.ToString(), oneC.Error);
            })
            .ToList();
        var latest = (await orders
                .OrderByDescending(x => x.row.Order.CreatedAt).ThenByDescending(x => x.row.Order.Id)
                .Take(WorkspaceOverview.ListSize)
                .Select(x => new
                {
                    x.row.Order.Id, x.row.Order.OrderNumber, x.row.Order.CounterpartyId, x.customer.Name,
                    x.row.Order.CreatedAt, LineCount = x.row.Order.Lines.Count,
                    Total = x.row.Order.Lines.Sum(y => y.Amount),
                    Adjustment = x.row.Order.Lines.Sum(y => y.PriceAdjustment * y.Quantity),
                    x.row.Order.OneCDocumentNumber, x.row.Order.OneCRejection, x.row.Message
                })
                .ToListAsync(ct))
            .Select(x =>
            {
                var oneC = OrderOneCQueries.OneC(x.OneCDocumentNumber, x.OneCRejection, x.Message);
                return new WorkspaceOrderListItem(x.Id.Value, x.OrderNumber, x.CounterpartyId.Value, x.Name, x.CreatedAt,
                    x.LineCount, x.Total, oneC.Status.ToString(), x.Adjustment,
                    oneC.Error, oneC.Attempts, oneC.LastAttemptAtUtc, oneC.NextAttemptAtUtc);
            })
            .ToList();

        return new WorkspaceOverview(ordersToday, draftCount, problemCount, problemRows, latest);
    }
}
