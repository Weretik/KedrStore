namespace Sales.Api.Endpoints.Orders.GetOrderById;

public sealed record GetAdminOrderDetailResponse(
    long OrderId,
    string OrderNumber,
    DateTimeOffset CreatedAtUtc,
    GetAdminOrderCounterpartyResponse Counterparty,
    string? Comment,
    IReadOnlyList<GetAdminOrderLineResponse> Lines,
    decimal TotalAmount,
    GetAdminOrderSyncResponse Sync);
