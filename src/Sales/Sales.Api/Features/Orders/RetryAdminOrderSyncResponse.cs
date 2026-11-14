namespace Sales.Api.Features.Orders;

public sealed record RetryAdminOrderSyncResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus);
