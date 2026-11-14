namespace Sales.Api.Features.Orders;

public sealed record RetryAdminOrderSyncRequest
{
    public string Reason { get; init; } = string.Empty;
}
