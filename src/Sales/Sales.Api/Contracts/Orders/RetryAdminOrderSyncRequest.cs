namespace Sales.Api.Contracts.Orders;

public sealed record RetryAdminOrderSyncRequest
{
    public string Reason { get; init; } = string.Empty;
}
