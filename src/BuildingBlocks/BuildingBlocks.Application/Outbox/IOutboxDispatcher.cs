namespace BuildingBlocks.Application.Outbox;

/// <summary>Sends stored outbox messages of every module.</summary>
public interface IOutboxDispatcher
{
    /// <summary>
    /// The one attempt right after the commit of the request that stored these messages. It never throws for a failed
    /// send: the message stays for the next dispatch.
    /// </summary>
    Task DispatchAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken cancellationToken);

    /// <summary>
    /// Sends every due message that is not in a lane (the scheduled dispatch) and removes processed messages past
    /// retention. Messages of a lane wait for their own <see cref="DispatchLaneAsync"/>.
    /// </summary>
    Task<OutboxDispatchReport> DispatchDueAsync(CancellationToken cancellationToken);

    /// <summary>Sends the due messages of one lane only; it is the lane's own background task, so pausing it holds them.</summary>
    Task<OutboxDispatchReport> DispatchLaneAsync(string lane, CancellationToken cancellationToken);
}

/// <summary>
/// Lanes: message types sent by their own background task instead of the general outbox task, so the admin can pause
/// or reschedule them separately. Also the key of that task.
/// </summary>
public static class OutboxLanes
{
    /// <summary>Orders sent to 1C.</summary>
    public const string OneCOrders = "one-c-orders";
}
