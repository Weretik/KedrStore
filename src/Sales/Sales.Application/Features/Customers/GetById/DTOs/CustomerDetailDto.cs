namespace Sales.Application.Features.Customers.GetById.DTOs;

public sealed record CustomerDetailDto(
    string CounterpartyId,
    string Name,
    string? Phone,
    string Email,
    int DefaultPriceTypeId,
    IReadOnlyList<CustomerCategoryPriceTypeDto> CategoryPriceTypes);
