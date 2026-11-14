namespace Sales.Application.Features.Customers.Queries.GetCustomerById;

public sealed record CustomerCategoryPriceTypeDto(
    int CategoryId,
    int PriceTypeId,
    string? CategoryName = null,
    string? PriceTypeName = null);
