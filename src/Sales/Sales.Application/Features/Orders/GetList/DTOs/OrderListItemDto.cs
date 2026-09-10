using Sales.Domain.Orders.Enums;

namespace Sales.Application.Features.Orders.GetList.DTOs;

public sealed record OrderListItemDto(
    long OrderId,
    string OrderNumber,
    string CounterpartyId,
    string CounterpartyName,
    DateTimeOffset CreatedAtUtc,
    int LineCount,
    decimal TotalAmount,
    OneCOrderSyncStatus SyncStatus,
    string? OneCDocumentNumber);
