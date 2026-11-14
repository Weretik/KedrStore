namespace Sales.Api.Endpoints.Orders.RetryOrderSync;

public sealed record RetryAdminOrderSyncRequest
{
    public string Reason { get; init; } = string.Empty;
}
