using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.GetById;

public sealed record GetWorkspaceOrderQuery(long OrderId) : IQuery<Result<WorkspaceOrderDetail>>;
