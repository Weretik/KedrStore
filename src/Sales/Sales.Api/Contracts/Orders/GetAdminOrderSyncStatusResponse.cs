namespace Sales.Api.Contracts.Orders;

public sealed record GetAdminOrderSyncStatusResponse(
    long OrderId,
    string OrderNumber,
    string SyncStatus,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
