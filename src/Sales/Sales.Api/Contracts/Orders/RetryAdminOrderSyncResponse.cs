namespace Sales.Api.Contracts.Orders;

public sealed record RetryAdminOrderSyncResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus);
