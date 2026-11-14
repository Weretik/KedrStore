namespace Sales.Api.Endpoints.Orders.CreateOrder;

public sealed record CreateAdminOrderResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus);
