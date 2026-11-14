namespace BuildingBlocks.Application.Outbox;

/// <summary>
/// Sends one stored message. An exception means "not delivered": the outbox keeps the message and tries again later.
/// The message id is stable across attempts, so a receiver that accepts a key can ignore a repeat.
/// </summary>
public interface IOutboxMessageHandler<in TMessage>
    where TMessage : IOutboxMessage
{
    Task HandleAsync(TMessage message, Guid messageId, CancellationToken cancellationToken);
}
