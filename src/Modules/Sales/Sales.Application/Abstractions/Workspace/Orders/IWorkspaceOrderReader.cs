namespace Sales.Application.Abstractions.Workspace.Orders;

public interface IWorkspaceOrderReader
{
    Task<WorkspacePage<WorkspaceOrderListItem>> GetOrdersAsync(string? searchTerm, string? counterpartyId,
        DateTimeOffset? createdFromUtc, DateTimeOffset? createdToUtcExclusive,
        string sort, int page, int pageSize, CancellationToken cancellationToken);
    Task<WorkspaceOrderDetail?> GetOrderAsync(long orderId, CancellationToken cancellationToken);
}
