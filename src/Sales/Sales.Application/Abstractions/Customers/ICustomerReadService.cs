using Sales.Application.Features.Customers.Queries.GetCustomerById;
using Sales.Application.Features.Customers.Queries.GetCustomerList;

namespace Sales.Application.Abstractions.Customers;

public interface ICustomerReadService
{
    Task<Result<PagedResult<List<CustomerListItemDto>>>> GetListAsync(
        CustomerListRequest request,
        CancellationToken cancellationToken);

    Task<Result<CustomerDetailDto>> GetByIdAsync(
        string counterpartyId,
        CancellationToken cancellationToken);
}
