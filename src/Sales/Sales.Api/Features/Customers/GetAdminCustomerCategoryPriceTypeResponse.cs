namespace Sales.Api.Features.Customers;

public sealed record GetAdminCustomerCategoryPriceTypeResponse(
    int CategoryId,
    int PriceTypeId,
    string? CategoryName,
    string? PriceTypeName);
