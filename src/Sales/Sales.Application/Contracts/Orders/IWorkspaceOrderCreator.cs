namespace Sales.Application.Contracts.Orders;

public interface IWorkspaceOrderCreator
{
    Task<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)> CreateOrderAsync(WorkspaceCreateOrderRequest request, Guid idempotencyKey, CancellationToken cancellationToken);
}
