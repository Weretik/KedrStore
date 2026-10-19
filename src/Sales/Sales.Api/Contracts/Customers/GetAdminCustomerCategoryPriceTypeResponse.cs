namespace Sales.Api.Contracts.Customers;

public sealed record GetAdminCustomerCategoryPriceTypeResponse(
    int CategoryId,
    int PriceTypeId,
    string? CategoryName,
    string? PriceTypeName);
