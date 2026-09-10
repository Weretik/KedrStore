namespace Sales.Api.Contracts.Customers;

public sealed record GetAdminCustomerCategoryPriceTypeResponse(
    int CategoryId,
    int PriceTypeId);
