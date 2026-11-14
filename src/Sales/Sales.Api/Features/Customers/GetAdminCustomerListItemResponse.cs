namespace Sales.Api.Features.Customers;

public sealed record GetAdminCustomerListItemResponse(
    string CounterpartyId,
    string Name,
    string? Phone);
