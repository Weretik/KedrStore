namespace Sales.Api.Contracts.Orders;

public sealed record GetAdminOrderSyncResponse(
    string Status,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
