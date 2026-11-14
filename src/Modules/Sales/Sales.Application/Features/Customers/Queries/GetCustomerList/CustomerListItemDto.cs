namespace Sales.Application.Features.Customers.Queries.GetCustomerList;

public sealed record CustomerListItemDto(
    string CounterpartyId,
    string Name,
    string? Phone);
