using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Application.Abstractions.Persistence;

public sealed record CreateOrderIdempotencyRecord(
    string Operation,
    string IdempotencyKey,
    string RequestHash,
    OrderId OrderId,
    string OrderNumber,
    OneCOrderSyncStatus SyncStatus,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc);
