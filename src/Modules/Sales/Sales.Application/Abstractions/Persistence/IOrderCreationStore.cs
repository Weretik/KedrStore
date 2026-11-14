using Sales.Domain.Orders;

namespace Sales.Application.Abstractions.Persistence;

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
