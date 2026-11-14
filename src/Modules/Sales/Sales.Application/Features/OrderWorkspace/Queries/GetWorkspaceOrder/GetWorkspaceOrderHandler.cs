using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Orders;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrder;

public sealed class GetWorkspaceOrderHandler(IWorkspaceOrderReader port)
    : IQueryHandler<GetWorkspaceOrderQuery, Result<WorkspaceOrderDetail>>
{
    public async ValueTask<Result<WorkspaceOrderDetail>> Handle(GetWorkspaceOrderQuery request, CancellationToken cancellationToken)
    {
        var value = await port.GetOrderAsync(request.OrderId, cancellationToken);
        return value is null ? Result.NotFound() : Result.Success(value);
    }
}
