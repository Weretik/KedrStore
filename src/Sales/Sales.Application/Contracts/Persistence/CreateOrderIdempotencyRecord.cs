using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Application.Contracts.Persistence;

public sealed record CreateOrderIdempotencyRecord(
    string Operation,
    string IdempotencyKey,
    string RequestHash,
    OrderId OrderId,
    string OrderNumber,
    OneCOrderSyncStatus SyncStatus,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc);
