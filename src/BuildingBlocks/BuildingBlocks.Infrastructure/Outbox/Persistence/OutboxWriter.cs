using System.Text.Json;
using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;
namespace BuildingBlocks.Infrastructure.Outbox.Persistence;

/// <summary>
/// Adds a message to a module's <see cref="DbContext"/>, so it is saved in the same <c>SaveChanges</c> as the data
/// (platform spec 006, R-002).
/// </summary>
public sealed class OutboxWriter(OutboxMessageTypes types, TimeProvider clock)
{
    /// <param name="id">
    /// A message id the caller already has, when the message stands for one business fact that must be stored once (a
    /// quick order is stored under its order id, so the primary key rejects a repeat). Otherwise a new id.
    /// </param>
    /// <returns>The message id, for the attempt right after the commit.</returns>
    public Guid Add(DbContext dbContext, IOutboxMessage message, Guid? id = null)
    {
        var type = types.Of(message);
        var payload = JsonSerializer.Serialize(message, type.MessageType, OutboxMessageType.Json);
        var row = OutboxMessage.Create(id ?? Guid.CreateVersion7(), type.Name, payload, clock.GetUtcNow());
        dbContext.Set<OutboxMessage>().Add(row);
        return row.Id;
    }

    /// <summary>
    /// Puts a finished message (processed or failed) back in the queue with zero attempts, for a manual retry
    /// (platform spec 006, R-013). The update is conditional, so of two retries at once only one wins; a message
    /// removed by retention is stored again under the same id, and then the primary key rejects a racing copy. Run it
    /// in the caller's transaction: the update is sent at once, a re-added row with the caller's <c>SaveChanges</c>.
    /// </summary>
    /// <returns>False when the message is still waiting or being sent: there is nothing to retry.</returns>
    public async Task<bool> RequeueAsync(
        DbContext dbContext,
        IOutboxMessage message,
        Guid id,
        CancellationToken cancellationToken)
    {
        var nowUtc = clock.GetUtcNow();
        var messages = dbContext.Set<OutboxMessage>();
        var reset = await messages
            .Where(row => row.Id == id && (row.ProcessedAtUtc != null || row.FailedAtUtc != null))
            .ExecuteUpdateAsync(row => row
                .SetProperty(x => x.Attempts, 0)
                .SetProperty(x => x.ProcessedAtUtc, (DateTimeOffset?)null)
                .SetProperty(x => x.FailedAtUtc, (DateTimeOffset?)null)
                .SetProperty(x => x.LastError, (string?)null)
                .SetProperty(x => x.LastAttemptAtUtc, (DateTimeOffset?)null)
                .SetProperty(x => x.NextAttemptAtUtc, nowUtc), cancellationToken);
        if (reset == 1)
            return true;

        if (await messages.AnyAsync(row => row.Id == id, cancellationToken))
            return false;

        Add(dbContext, message, id);
        return true;
    }
}
