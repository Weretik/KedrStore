using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Orders;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrder;

public sealed record GetWorkspaceOrderQuery(long OrderId) : IQuery<Result<WorkspaceOrderDetail>>;
