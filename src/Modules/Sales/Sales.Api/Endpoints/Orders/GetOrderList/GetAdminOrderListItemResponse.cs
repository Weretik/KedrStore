namespace Sales.Api.Endpoints.Orders.GetOrderList;

public sealed record GetAdminOrderListItemResponse(
    long OrderId,
    string OrderNumber,
    string CounterpartyId,
    string CounterpartyName,
    DateTimeOffset CreatedAtUtc,
    int LineCount,
    decimal TotalAmount,
    string SyncStatus,
    string? OneCDocumentNumber);
