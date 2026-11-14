namespace BuildingBlocks.Application.Outbox;

/// <summary>Sends stored outbox messages of every module.</summary>
public interface IOutboxDispatcher
{
    /// <summary>
    /// The one attempt right after the commit of the request that stored these messages. It never throws for a failed
    /// send: the message stays for the next dispatch.
    /// </summary>
    Task DispatchAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken cancellationToken);

    /// <summary>Sends every due message (the scheduled dispatch) and removes processed messages past retention.</summary>
    Task<OutboxDispatchReport> DispatchDueAsync(CancellationToken cancellationToken);
}
