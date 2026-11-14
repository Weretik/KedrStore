using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.CustomerPrices;

public sealed class GetWorkspaceCustomerPricesQueryHandler(IWorkspacePricing port)
    : IQueryHandler<GetWorkspaceCustomerPricesQuery, Result<WorkspaceCustomerPrices>>
{
    public async ValueTask<Result<WorkspaceCustomerPrices>> Handle(GetWorkspaceCustomerPricesQuery request, CancellationToken cancellationToken)
        => (await port.GetCustomerPricesAsync(request.Request, cancellationToken)).AsResult();
}
