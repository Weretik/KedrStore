using Sales.Application.Features.Customers.Queries.GetCustomerList;

namespace Sales.Application.Features.Customers.Queries.GetCustomerList;

public sealed record GetCustomerListQuery(CustomerListRequest Request)
    : IQuery<Result<PagedResult<List<CustomerListItemDto>>>>;
