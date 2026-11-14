namespace Sales.Api.Endpoints.Customers;

public sealed record GetAdminCustomerListItemResponse(
    string CounterpartyId,
    string Name,
    string? Phone);
