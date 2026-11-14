namespace Sales.Api.Features.Orders;

public sealed record GetAdminOrderSyncResponse(
    string Status,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
