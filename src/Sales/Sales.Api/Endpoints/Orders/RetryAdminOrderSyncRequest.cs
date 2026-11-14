namespace Sales.Api.Endpoints.Orders;

public sealed record RetryAdminOrderSyncRequest
{
    public string Reason { get; init; } = string.Empty;
}
