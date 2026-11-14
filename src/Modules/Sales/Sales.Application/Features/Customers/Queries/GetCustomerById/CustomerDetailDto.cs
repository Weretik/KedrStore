namespace Sales.Application.Features.Customers.Queries.GetCustomerById;

public sealed record CustomerDetailDto(
    string CounterpartyId,
    string Name,
    string? Phone,
    string Email,
    int DefaultPriceTypeId,
    IReadOnlyList<CustomerCategoryPriceTypeDto> CategoryPriceTypes,
    string? DefaultPriceTypeName = null);
