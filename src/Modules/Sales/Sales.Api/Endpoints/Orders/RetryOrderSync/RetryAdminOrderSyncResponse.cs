namespace Sales.Api.Endpoints.Orders.RetryOrderSync;

public sealed record RetryAdminOrderSyncResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus);
