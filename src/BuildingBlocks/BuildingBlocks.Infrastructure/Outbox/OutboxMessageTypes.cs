using BuildingBlocks.Application.Outbox;

namespace BuildingBlocks.Infrastructure.Outbox;

/// <summary>Registered message types: the stored name, the CLR type and how to run its handler.</summary>
public sealed class OutboxMessageTypes
{
    private readonly Dictionary<string, OutboxMessageType> _byName = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, OutboxMessageType> _byType = new();

    public OutboxMessageTypes(IEnumerable<OutboxMessageType> types)
    {
        foreach (var type in types)
        {
            if (!_byName.TryAdd(type.Name, type) || !_byType.TryAdd(type.MessageType, type))
                throw new InvalidOperationException($"Outbox message type '{type.Name}' is registered twice.");
        }
    }

    public OutboxMessageType Of(IOutboxMessage message)
        => _byType.TryGetValue(message.GetType(), out var type)
            ? type
            : throw new InvalidOperationException(
                $"Outbox message {message.GetType().Name} has no handler; register it with AddOutboxHandler.");

    public OutboxMessageType? Find(string name) => _byName.GetValueOrDefault(name);
}
