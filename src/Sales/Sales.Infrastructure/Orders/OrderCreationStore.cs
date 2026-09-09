using Sales.Application.Contracts.Persistence;
using Sales.Infrastructure.DataBase;
using Sales.Infrastructure.DataBase.Entities;

namespace Sales.Infrastructure.Orders;

internal sealed class OrderCreationStore(SalesDbContext dbContext) : IOrderCreationStore
{
    public Task<bool> CounterpartyExistsAsync(string counterpartyId, CancellationToken cancellationToken)
        => dbContext.Counterparties.AnyAsync(counterparty => counterparty.Id == counterpartyId && !counterparty.IsDeleted, cancellationToken);

    public Task<CreateOrderIdempotencyRecord?> FindIdempotencyRecordAsync(string operation, string idempotencyKey, CancellationToken cancellationToken)
        => (from record in dbContext.OrderIdempotencyRecords.AsNoTracking()
            join order in dbContext.Orders.AsNoTracking() on record.OrderId equals order.Id
            join synchronization in dbContext.OneCOrderSyncs.AsNoTracking() on order.Id equals synchronization.OrderId
            where record.Operation == operation && record.IdempotencyKey == idempotencyKey
            select new CreateOrderIdempotencyRecord(
                record.Operation, record.IdempotencyKey, record.RequestHash, record.OrderId,
                order.OrderNumber, synchronization.Status, record.CreatedAtUtc, record.ExpiresAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task PersistAsync(
        Order order,
        Func<OrderId, OneCOrderSync> synchronizationFactory,
        Func<OrderId, CreateOrderIdempotencyRecord> idempotencyRecordFactory,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Orders.AddAsync(order, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        order.AssignOrderNumber($"SO-{order.CreatedAt:yyyyMMdd}-{order.Id.Value}");

        var synchronization = synchronizationFactory(order.Id);
        var idempotency = idempotencyRecordFactory(order.Id);
        await dbContext.OneCOrderSyncs.AddAsync(synchronization, cancellationToken);
        await dbContext.OrderIdempotencyRecords.AddAsync(new OrderIdempotencyRecord
        {
            Operation = idempotency.Operation,
            IdempotencyKey = idempotency.IdempotencyKey,
            RequestHash = idempotency.RequestHash,
            OrderId = idempotency.OrderId,
            CreatedAtUtc = idempotency.CreatedAtUtc,
            ExpiresAtUtc = idempotency.ExpiresAtUtc
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
