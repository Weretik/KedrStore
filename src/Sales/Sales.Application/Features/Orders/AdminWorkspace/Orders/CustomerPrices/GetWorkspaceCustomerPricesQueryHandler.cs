using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.CustomerPrices;

public sealed class GetWorkspaceCustomerPricesQueryHandler(IWorkspacePricing port)
    : IQueryHandler<GetWorkspaceCustomerPricesQuery, Result<WorkspaceCustomerPrices>>
{
    public async ValueTask<Result<WorkspaceCustomerPrices>> Handle(GetWorkspaceCustomerPricesQuery request, CancellationToken cancellationToken)
    {
        return Result.Success(await port.GetCustomerPricesAsync(request.Request, cancellationToken));
    }
}
