using Sales.Application.Features.Customers.GetById.DTOs;
using Sales.Application.Features.Customers.GetList.DTOs;

namespace Sales.Application.Contracts.Customers;

public interface ICustomerReadService
{
    Task<Result<PagedResult<List<CustomerListItemDto>>>> GetListAsync(
        CustomerListRequest request,
        CancellationToken cancellationToken);

    Task<Result<CustomerDetailDto>> GetByIdAsync(
        string counterpartyId,
        CancellationToken cancellationToken);
}
