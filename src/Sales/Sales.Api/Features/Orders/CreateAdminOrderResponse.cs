namespace Sales.Api.Features.Orders;

public sealed record CreateAdminOrderResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus);
