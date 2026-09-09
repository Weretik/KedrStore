namespace Sales.Api.Contracts.Orders;

public sealed record CreateAdminOrderResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus);
