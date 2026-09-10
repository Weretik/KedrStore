using Sales.Application.Features.Customers.GetList.DTOs;

namespace Sales.Application.Features.Customers.GetList;

public sealed record GetCustomerListQuery(CustomerListRequest Request)
    : IQuery<Result<PagedResult<List<CustomerListItemDto>>>>;
