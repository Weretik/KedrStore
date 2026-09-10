namespace Sales.Api.Contracts.Customers;

public sealed record GetAdminCustomerListItemResponse(
    string CounterpartyId,
    string Name,
    string? Phone);
