using Sales.Application.Abstractions.Customers;
using Sales.Application.Features.Customers.Queries.GetCustomerById;

namespace Sales.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdHandler(ICustomerReadService reader)
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
