using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.GetById;

public sealed class GetWorkspaceOrderQueryHandler(IWorkspaceOrderReader port)
    : IQueryHandler<GetWorkspaceOrderQuery, Result<WorkspaceOrderDetail>>
{
    public async ValueTask<Result<WorkspaceOrderDetail>> Handle(GetWorkspaceOrderQuery request, CancellationToken cancellationToken)
    {
        var value = await port.GetOrderAsync(request.OrderId, cancellationToken);
        return value is null ? Result.NotFound() : Result.Success(value);
    }
}
