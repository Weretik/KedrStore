using Sales.Application.Contracts.Customers;
using Sales.Application.Features.Customers.GetById.DTOs;

namespace Sales.Application.Features.Customers.GetById;

public sealed class GetCustomerByIdQueryHandler(ICustomerReadService reader)
    : IQueryHandler<GetCustomerByIdQuery, Result<CustomerDetailDto>>
{
    public async ValueTask<Result<CustomerDetailDto>> Handle(
        GetCustomerByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await reader.GetByIdAsync(query.CounterpartyId, cancellationToken);
    }
}
