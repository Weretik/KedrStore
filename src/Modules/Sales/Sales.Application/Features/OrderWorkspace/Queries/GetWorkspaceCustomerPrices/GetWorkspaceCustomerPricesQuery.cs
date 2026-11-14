using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Pricing;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceCustomerPrices;

public sealed record GetWorkspaceCustomerPricesQuery(WorkspaceCustomerPricesRequest Request) : IQuery<Result<WorkspaceCustomerPrices>>;
