namespace Sales.Api.Contracts.Customers;

public sealed record GetAdminCustomerDetailResponse(
    string CounterpartyId,
    string Name,
    string? Phone,
    string Email,
    int DefaultPriceTypeId,
    IReadOnlyList<GetAdminCustomerCategoryPriceTypeResponse> CategoryPriceTypes);
