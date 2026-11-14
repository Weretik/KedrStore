using Sales.Application.Abstractions.Orders;

namespace Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceCustomerPrices;

public sealed record GetWorkspaceCustomerPricesQuery(WorkspaceCustomerPricesRequest Request) : IQuery<Result<WorkspaceCustomerPrices>>;
