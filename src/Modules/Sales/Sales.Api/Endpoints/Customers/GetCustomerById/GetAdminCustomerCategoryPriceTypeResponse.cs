namespace Sales.Api.Endpoints.Customers.GetCustomerById;

public sealed record GetAdminCustomerCategoryPriceTypeResponse(
    int CategoryId,
    int PriceTypeId,
    string? CategoryName,
    string? PriceTypeName);
