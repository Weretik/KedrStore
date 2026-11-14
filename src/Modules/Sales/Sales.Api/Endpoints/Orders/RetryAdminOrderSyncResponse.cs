namespace Sales.Api.Endpoints.Orders;

public sealed record RetryAdminOrderSyncResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus);
