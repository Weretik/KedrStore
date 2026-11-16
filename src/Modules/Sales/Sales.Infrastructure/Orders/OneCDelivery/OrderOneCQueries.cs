using System.Linq.Expressions;
using BuildingBlocks.Infrastructure.Outbox.Persistence;
using Sales.Application.Abstractions.Orders;

namespace Sales.Infrastructure.Orders;

/// <summary>
/// Every order reader takes the 1C state from here (platform spec 006, R-012): the order joined to its outbox message
/// by <c>OneCMessageId</c> (the primary key), and the state computed from both.
/// </summary>
internal static class OrderOneCQueries
{
    /// <summary>Orders with a 1C problem a manager can retry: refused by 1C, or the message gave up.</summary>
    public static readonly Expression<Func<OrderWithOneCMessage, bool>> IsProblem = x =>
        x.Order.OneCDocumentNumber == null &&
        (x.Order.OneCRejection != null || (x.Message != null && x.Message.FailedAtUtc != null));

    public static IQueryable<OrderWithOneCMessage> OrdersWithOneCMessage(this SalesDbContext db) =>
        from order in db.Orders.AsNoTracking()
        join message in db.Set<OutboxMessage>().AsNoTracking() on order.OneCMessageId equals message.Id into messages
        from message in messages.DefaultIfEmpty()
        select new OrderWithOneCMessage { Order = order, Message = message };

    /// <summary>The state of one loaded row; used in the final projection of a query.</summary>
    public static OrderOneCSync OneC(string? documentNumber, string? rejection, OutboxMessage? message) =>
        OrderOneCSync.From(documentNumber, rejection, message is null
            ? null
            : new OneCMessageState(message.Attempts, message.NextAttemptAtUtc, message.LastAttemptAtUtc,
                message.ProcessedAtUtc, message.FailedAtUtc, message.LastError));
}
