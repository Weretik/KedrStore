using Sales.Domain.Orders.Enums;

namespace Sales.Application.Features.Orders.GetSyncStatus.DTOs;

public sealed record GetOrderSyncStatusResult(
    long OrderId,
    string OrderNumber,
    OneCOrderSyncStatus SyncStatus,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
