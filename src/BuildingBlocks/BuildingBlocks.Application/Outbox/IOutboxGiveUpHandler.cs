namespace BuildingBlocks.Application.Outbox;

/// <summary>
/// Runs once when the last allowed attempt of a message failed (platform spec 006, R-009), in the dispatch
/// transaction: it may store follow-up messages, such as a signal to people. Its failure is logged and the message
/// stays failed.
/// </summary>
public interface IOutboxGiveUpHandler<in TMessage>
    where TMessage : IOutboxMessage
{
    Task HandleGiveUpAsync(TMessage message, OutboxGiveUp giveUp, CancellationToken cancellationToken);
}

/// <param name="MessageId">The failed message.</param>
/// <param name="Attempts">How many attempts were made.</param>
/// <param name="LastError">The error of the last attempt.</param>
public sealed record OutboxGiveUp(Guid MessageId, int Attempts, string? LastError);
