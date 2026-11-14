using Sales.Application.Abstractions.Customers;
using Sales.Application.Features.Customers.Queries.GetCustomerList;

namespace Sales.Application.Features.Customers.Queries.GetCustomerList;

public sealed class GetCustomerListHandler(ICustomerReadService reader)
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
