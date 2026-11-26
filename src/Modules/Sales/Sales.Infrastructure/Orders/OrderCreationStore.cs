using BuildingBlocks.Infrastructure.Outbox.Persistence;
using BuildingBlocks.Infrastructure.Persistence;
using Sales.Application.Abstractions.Persistence;
using Sales.Application.Features.Orders.OneCDelivery.Messages;
using Sales.Infrastructure.Persistence;
using Sales.Domain.Orders.ValueObjects;
using Sales.Infrastructure.Persistence.Orders.Idempotency;

namespace Sales.Infrastructure.Orders;

internal sealed class OrderCreationStore(SalesDbContext dbContext, OutboxWriter outbox, TimeProvider clock)
    : IOrderCreationStore
{
    public Task<bool> CounterpartyExistsAsync(string counterpartyId, CancellationToken cancellationToken)
        => dbContext.Counterparties.AnyAsync(counterparty => counterparty.Id == CounterpartyId.FromStorage(counterpartyId) && !counterparty.IsDeleted, cancellationToken);

    public async Task<CreateOrderIdempotencyRecord?> FindIdempotencyRecordAsync(string operation, string idempotencyKey, CancellationToken cancellationToken)
    {
        var row = await (
                from record in dbContext.OrderIdempotencyRecords.AsNoTracking()
                join order in dbContext.OrdersWithOneCMessage() on record.OrderId equals order.Order.Id
                where record.Operation == operation && record.IdempotencyKey == idempotencyKey
                select new
                {
                    Record = record,
                    order.Order.OrderNumber,
                    order.Order.OneCDocumentNumber,
                    order.Order.OneCRejection,
                    order.Message
                })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new CreateOrderIdempotencyRecord(row.Record.Operation, row.Record.IdempotencyKey, row.Record.RequestHash,
                row.Record.OrderId, row.OrderNumber,
                OrderOneCQueries.OneC(row.OneCDocumentNumber, row.OneCRejection, row.Message).Status,
                row.Record.CreatedAtUtc, row.Record.ExpiresAtUtc);
    }

    public async Task<bool> PersistAsync(
        Order order,
        Func<OrderId, CreateOrderIdempotencyRecord> idempotencyRecordFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            await PersistInTransactionAsync(order, idempotencyRecordFactory, cancellationToken);
            return true;
        }
        catch (DbUpdateException error) when (UniqueViolation.Is(error))
        {
            dbContext.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task AddFromOutboxAsync(Order order, CancellationToken cancellationToken)
    {
        // Inside the dispatch transaction EF Core saves under a savepoint, so a failed save leaves the batch usable.
        await dbContext.Orders.AddAsync(order, cancellationToken);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            dbContext.Entry(order).State = EntityState.Detached;
            foreach (var line in order.Lines)
                dbContext.Entry(line).State = EntityState.Detached;
            throw;
        }

        AssignNumberAndQueue(order);
    }

    private void AssignNumberAndQueue(Order order)
    {
        order.AssignOrderNumber($"SO-{order.CreatedAt:yyyyMMdd}-{order.Id.Value}");
        // Platform spec 006, R-010: the 1C delivery is stored with the order; the scheduled dispatch sends it.
        outbox.Add(dbContext, new OrderPlaced(order.Id.Value), order.OneCMessageId);
    }

    private async Task PersistInTransactionAsync(
        Order order,
        Func<OrderId, CreateOrderIdempotencyRecord> idempotencyRecordFactory,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        // The 24-hour idempotency records were cleared by the old 1C job; each new order clears the expired ones now.
        await dbContext.OrderIdempotencyRecords
            .Where(record => record.ExpiresAtUtc <= clock.GetUtcNow())
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.Orders.AddAsync(order, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        AssignNumberAndQueue(order);

        var idempotency = idempotencyRecordFactory(order.Id);
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
