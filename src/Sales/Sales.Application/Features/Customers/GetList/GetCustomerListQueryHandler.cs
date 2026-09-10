using Sales.Application.Contracts.Customers;
using Sales.Application.Features.Customers.GetList.DTOs;

namespace Sales.Application.Features.Customers.GetList;

public sealed class GetCustomerListQueryHandler(ICustomerReadService reader)
    : IQueryHandler<GetCustomerListQuery, Result<PagedResult<List<CustomerListItemDto>>>>
{
    public async ValueTask<Result<PagedResult<List<CustomerListItemDto>>>> Handle(
        GetCustomerListQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await reader.GetListAsync(query.Request, cancellationToken);
    }
}
