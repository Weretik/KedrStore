using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;

namespace Sales.Infrastructure.QuickOrders;

/// <summary>A quick order is only an outbox message: the managers work it from the Excel file in Telegram.</summary>
internal sealed class QuickOrderStore(SalesDbContext dbContext, OutboxWriter outbox) : IQuickOrderStore
{
    public async Task<Guid> SaveAsync(QuickOrderPlaced order, CancellationToken cancellationToken)
    {
        var messageId = outbox.Add(dbContext, order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return messageId;
    }
}
