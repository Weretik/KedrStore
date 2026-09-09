using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Application.Contracts.Persistence;

public interface IOrderCreationStore
{
    Task<bool> CounterpartyExistsAsync(string counterpartyId, CancellationToken cancellationToken);

    Task<CreateOrderIdempotencyRecord?> FindIdempotencyRecordAsync(
        string operation,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task PersistAsync(
        Order order,
        Func<OrderId, OneCOrderSync> synchronizationFactory,
        Func<OrderId, CreateOrderIdempotencyRecord> idempotencyRecordFactory,
        CancellationToken cancellationToken);
}
