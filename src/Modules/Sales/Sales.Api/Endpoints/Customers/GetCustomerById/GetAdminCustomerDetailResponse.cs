namespace Sales.Api.Endpoints.Customers.GetCustomerById;

public sealed record GetAdminCustomerDetailResponse(
    string CounterpartyId,
    string Name,
    string? Phone,
    string Email,
    int DefaultPriceTypeId,
    IReadOnlyList<GetAdminCustomerCategoryPriceTypeResponse> CategoryPriceTypes,
    string? DefaultPriceTypeName);
