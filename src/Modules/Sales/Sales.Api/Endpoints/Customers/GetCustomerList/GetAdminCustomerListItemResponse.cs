namespace Sales.Api.Endpoints.Customers.GetCustomerList;

public sealed record GetAdminCustomerListItemResponse(
    string CounterpartyId,
    string Name,
    string? Phone);
