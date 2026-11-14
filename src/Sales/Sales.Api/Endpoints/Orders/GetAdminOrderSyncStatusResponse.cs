namespace Sales.Api.Endpoints.Orders;

public sealed record GetAdminOrderSyncStatusResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
