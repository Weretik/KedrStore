using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.Create;

public sealed record CreateWorkspaceOrderCommand(WorkspaceCreateOrderRequest Order, Guid IdempotencyKey) : ICommand<Result<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>>;
