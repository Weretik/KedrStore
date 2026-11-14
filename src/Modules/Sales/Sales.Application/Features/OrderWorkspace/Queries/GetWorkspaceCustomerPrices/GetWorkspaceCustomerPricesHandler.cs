using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Pricing;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceCustomerPrices;

public sealed class GetWorkspaceCustomerPricesHandler(IWorkspacePricing port)
    : IQueryHandler<GetWorkspaceCustomerPricesQuery, Result<WorkspaceCustomerPrices>>
{
    public async ValueTask<Result<WorkspaceCustomerPrices>> Handle(GetWorkspaceCustomerPricesQuery request, CancellationToken cancellationToken)
        => (await port.GetCustomerPricesAsync(request.Request, cancellationToken)).AsResult();
}
