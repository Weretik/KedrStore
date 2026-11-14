using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Pricing;
using Sales.Application.Abstractions.Workspace.Orders;

namespace Sales.Application.Features.OrderWorkspace.Commands.CreateWorkspaceOrder;

public sealed record CreateWorkspaceOrderCommand(WorkspaceCreateOrderRequest Order, Guid IdempotencyKey) : ICommand<Result<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>>;
