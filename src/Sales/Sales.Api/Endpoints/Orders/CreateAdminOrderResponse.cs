namespace Sales.Api.Endpoints.Orders;

public sealed record CreateAdminOrderResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus);
