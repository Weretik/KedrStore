using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceOrder;

public sealed record CreateWorkspaceOrderCommand(WorkspaceCreateOrderRequest Order, Guid IdempotencyKey) : ICommand<Result<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>>;
