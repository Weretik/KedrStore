using Sales.Domain.Orders.Enums;

namespace Sales.Application.Features.Orders.RetrySync.DTOs;

public sealed record RetryOrderSyncResult(
    long OrderId,
    string OrderNumber,
    OneCOrderSyncStatus SyncStatus);
