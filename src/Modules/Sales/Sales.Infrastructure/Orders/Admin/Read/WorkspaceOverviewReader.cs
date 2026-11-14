using Sales.Application.Abstractions.Orders;
using Sales.Domain.Orders;

namespace Sales.Infrastructure.Orders;

/// <summary>Counts and short lists for the admin «Огляд»; a sync problem is a status the manual retry accepts.</summary>
internal sealed class WorkspaceOverviewReader(SalesDbContext db) : IWorkspaceOverviewReader
{
    public async Task<WorkspaceOverview> GetAsync(DateTimeOffset todayFromUtc, CancellationToken ct)
    {
        var orders =
            from order in db.Orders.AsNoTracking()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking() on order.CounterpartyId equals customer.Id
            join sync in db.OneCOrderSyncs.AsNoTracking() on order.Id equals sync.OrderId
            select new { order, customer, sync };
        var problems = orders.Where(x =>
            x.sync.Status == OneCOrderSyncStatus.BusinessError || x.sync.Status == OneCOrderSyncStatus.DeadLetter);

        var ordersToday = await db.Orders.AsNoTracking().CountAsync(order => order.CreatedAt >= todayFromUtc, ct);
        var draftCount = await db.OrderDrafts.AsNoTracking().CountAsync(ct);
        var problemCount = await problems.CountAsync(ct);
        var problemRows = await problems
            .OrderByDescending(x => x.order.CreatedAt).ThenByDescending(x => x.order.Id)
            .Take(WorkspaceOverview.ListSize)
            .Select(x => new WorkspaceSyncProblem(x.order.Id.Value, x.order.OrderNumber, x.customer.Name,
                x.order.CreatedAt, x.sync.Status.ToString(), x.sync.LastErrorMessage))
            .ToListAsync(ct);
        var latest = await orders
            .OrderByDescending(x => x.order.CreatedAt).ThenByDescending(x => x.order.Id)
            .Take(WorkspaceOverview.ListSize)
            .Select(x => new WorkspaceOrderListItem(x.order.Id.Value, x.order.OrderNumber,
                x.order.CounterpartyId, x.customer.Name, x.order.CreatedAt,
                x.order.Lines.Count, x.order.Lines.Sum(y => y.Amount), x.sync.Status.ToString(),
                x.order.Lines.Sum(y => y.PriceAdjustment * y.Quantity)))
            .ToListAsync(ct);

        return new WorkspaceOverview(ordersToday, draftCount, problemCount, problemRows, latest);
    }
}
