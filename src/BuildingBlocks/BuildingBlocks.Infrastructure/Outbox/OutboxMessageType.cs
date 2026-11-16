using System.Text.Json;
using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Outbox;

/// <summary>One registered message type and the call of its handler.</summary>
public sealed class OutboxMessageType(
    string name,
    Type messageType,
    Func<IServiceProvider, string, Guid, CancellationToken, Task<OutboxHandlingResult>> handle,
    Func<IServiceProvider, string, OutboxGiveUp, CancellationToken, Task> runGiveUp)
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public string Name { get; } = name;

    public Type MessageType { get; } = messageType;

    /// <summary>A stored payload read back, for a module that looks up its own message (a repeated request).</summary>
    public static TMessage? ReadPayload<TMessage>(string payload)
        where TMessage : IOutboxMessage
        => JsonSerializer.Deserialize<TMessage>(payload, Json);

    internal Task<OutboxHandlingResult> HandleAsync(
        IServiceProvider services,
        string payload,
        Guid messageId,
        CancellationToken cancellationToken)
        => handle(services, payload, messageId, cancellationToken);

    /// <summary>Runs the module's give-up handler of this type, when it registered one (R-009).</summary>
    internal Task GiveUpAsync(IServiceProvider services, string payload, OutboxGiveUp giveUp, CancellationToken cancellationToken)
        => runGiveUp(services, payload, giveUp, cancellationToken);

    internal static OutboxMessageType For<TMessage>(string name)
        where TMessage : IOutboxMessage
        => new(name, typeof(TMessage),
            (services, payload, messageId, cancellationToken) =>
                services.GetRequiredService<IOutboxMessageHandler<TMessage>>()
                    .HandleAsync(Read<TMessage>(payload, messageId), messageId, cancellationToken),
            (services, payload, giveUp, cancellationToken) =>
                services.GetService<IOutboxGiveUpHandler<TMessage>>() is { } handler
                    ? handler.HandleGiveUpAsync(Read<TMessage>(payload, giveUp.MessageId), giveUp, cancellationToken)
                    : Task.CompletedTask);

    private static TMessage Read<TMessage>(string payload, Guid messageId)
        where TMessage : IOutboxMessage
        => JsonSerializer.Deserialize<TMessage>(payload, Json)
           ?? throw new InvalidOperationException($"Outbox message {messageId} has an empty payload.");
}
