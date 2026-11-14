namespace Sales.Api.Endpoints.Orders.GetOrderById;

public sealed record GetAdminOrderSyncResponse(
    string Status,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
