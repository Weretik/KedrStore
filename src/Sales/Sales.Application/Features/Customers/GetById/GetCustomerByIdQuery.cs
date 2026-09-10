using Sales.Application.Features.Customers.GetById.DTOs;

namespace Sales.Application.Features.Customers.GetById;

public sealed record GetCustomerByIdQuery(string CounterpartyId) : IQuery<Result<CustomerDetailDto>>;
