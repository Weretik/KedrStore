namespace BuildingBlocks.Application.Outbox;

/// <summary>
/// Sends one stored message and answers how it went (platform spec 006, R-008). An exception also means "not
/// delivered": the outbox keeps the message and tries again later. The message id is stable across attempts, so a
/// receiver that accepts a key can ignore a repeat. Changes the handler makes to the module's tracked data are saved
/// with the outcome, in the dispatch transaction.
/// </summary>
public interface IOutboxMessageHandler<in TMessage>
    where TMessage : IOutboxMessage
{
    Task<OutboxHandlingResult> HandleAsync(TMessage message, Guid messageId, CancellationToken cancellationToken);
}
