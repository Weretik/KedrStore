using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Application.Abstractions.Persistence;

public interface IOrderCreationStore
{
    Task<bool> CounterpartyExistsAsync(string counterpartyId, CancellationToken cancellationToken);

    Task<CreateOrderIdempotencyRecord?> FindIdempotencyRecordAsync(
        string operation,
        string idempotencyKey,
        CancellationToken cancellationToken);

    /// <summary>
    /// Saves the order with its 1C outbox message (platform spec 006, R-010) and idempotency record in one transaction,
    /// removing expired idempotency records on the way. Answers false, saving nothing, when another request saved the
    /// same idempotency key meanwhile (database rules 4).
    /// </summary>
    Task<bool> PersistAsync(
        Order order,
        Func<OrderId, CreateOrderIdempotencyRecord> idempotencyRecordFactory,
        CancellationToken cancellationToken);
}
