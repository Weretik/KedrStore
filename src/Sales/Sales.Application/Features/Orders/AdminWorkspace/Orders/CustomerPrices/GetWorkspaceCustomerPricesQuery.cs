using Sales.Application.Contracts.Orders;

namespace Sales.Application.Features.Orders.AdminWorkspace.Orders.CustomerPrices;

public sealed record GetWorkspaceCustomerPricesQuery(WorkspaceCustomerPricesRequest Request) : IQuery<Result<WorkspaceCustomerPrices>>;
