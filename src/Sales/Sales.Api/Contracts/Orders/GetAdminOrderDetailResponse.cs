namespace Sales.Api.Contracts.Orders;

public sealed record GetAdminOrderDetailResponse(
    long OrderId,
    string OrderNumber,
    DateTimeOffset CreatedAtUtc,
    GetAdminOrderCounterpartyResponse Counterparty,
    string? Comment,
    IReadOnlyList<GetAdminOrderLineResponse> Lines,
    decimal TotalAmount,
    GetAdminOrderSyncResponse Sync);
