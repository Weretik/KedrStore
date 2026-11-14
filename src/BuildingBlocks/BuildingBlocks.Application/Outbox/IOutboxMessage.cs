namespace BuildingBlocks.Application.Outbox;

/// <summary>
/// A follow-up that a use case stores in its own transaction and the outbox sends after the commit: a record of plain
/// values (ids, names, numbers), never an entity.
/// </summary>
public interface IOutboxMessage;
