namespace Sales.Application.Contracts.Orders;

public interface IWorkspaceOrderCreator
{
    Task<DomainResult<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>> CreateOrderAsync(WorkspaceCreateOrderRequest request, Guid idempotencyKey, CancellationToken cancellationToken);
}
