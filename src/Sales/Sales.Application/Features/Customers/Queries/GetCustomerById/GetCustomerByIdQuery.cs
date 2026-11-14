using Sales.Application.Features.Customers.Queries.GetCustomerById;

namespace Sales.Application.Features.Customers.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(string CounterpartyId) : IQuery<Result<CustomerDetailDto>>;
