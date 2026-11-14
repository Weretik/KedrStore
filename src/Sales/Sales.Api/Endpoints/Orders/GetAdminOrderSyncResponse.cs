namespace Sales.Api.Endpoints.Orders;

public sealed record GetAdminOrderSyncResponse(
    string Status,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
