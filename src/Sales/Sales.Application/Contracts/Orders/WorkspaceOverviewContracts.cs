namespace Sales.Application.Contracts.Orders;

/// <summary>Orders block of the admin «Огляд».</summary>
/// <param name="OrdersToday">Orders created since the start of the viewer's day.</param>
/// <param name="DraftCount">Saved order drafts not turned into orders yet.</param>
/// <param name="SyncProblemCount">Orders 1C refused or gave up on (statuses a manager can retry).</param>
/// <param name="SyncProblems">The newest of them, at most <see cref="WorkspaceOverview.ListSize"/>.</param>
/// <param name="LatestOrders">The newest orders, at most <see cref="WorkspaceOverview.ListSize"/>.</param>
public sealed record WorkspaceOverview(
    int OrdersToday,
    int DraftCount,
    int SyncProblemCount,
    IReadOnlyList<WorkspaceSyncProblem> SyncProblems,
    IReadOnlyList<WorkspaceOrderListItem> LatestOrders)
{
    public const int ListSize = 5;
}

public sealed record WorkspaceSyncProblem(
    long OrderId,
    string OrderNumber,
    string CounterpartyName,
    DateTimeOffset CreatedAtUtc,
    string SyncStatus,
    string? ErrorMessage);
