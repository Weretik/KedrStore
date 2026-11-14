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
    /// <returns>The message id, for the attempt right after the commit.</returns>
    public Guid Add(DbContext dbContext, IOutboxMessage message)
    {
        var type = types.Of(message);
        var payload = JsonSerializer.Serialize(message, type.MessageType, OutboxMessageType.Json);
        var row = OutboxMessage.Create(Guid.CreateVersion7(), type.Name, payload, clock.GetUtcNow());
        dbContext.Set<OutboxMessage>().Add(row);
        return row.Id;
    }
}
