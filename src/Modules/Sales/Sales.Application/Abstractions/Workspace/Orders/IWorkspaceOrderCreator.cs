using Sales.Application.Abstractions.Workspace.Pricing;

namespace Sales.Application.Abstractions.Workspace.Orders;

public interface IWorkspaceOrderCreator
{
    Task<DomainResult<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>> CreateOrderAsync(WorkspaceCreateOrderRequest request, Guid idempotencyKey, CancellationToken cancellationToken);
}
