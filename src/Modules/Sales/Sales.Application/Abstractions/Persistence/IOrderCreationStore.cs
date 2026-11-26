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

    /// <summary>
    /// Adds an order made by an outbox handler, with its 1C outbox message, in the dispatch transaction (spec
    /// sales/009, R-010). It saves once to get the id for the order number; the dispatch saves the rest. On a failed
    /// save it forgets the order, so the dispatch still records the other messages.
    /// </summary>
    Task AddFromOutboxAsync(Order order, CancellationToken cancellationToken);
}
