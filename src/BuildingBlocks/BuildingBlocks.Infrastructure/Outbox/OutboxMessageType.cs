using System.Text.Json;
using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Outbox;

/// <summary>One registered message type and the call of its handler.</summary>
public sealed class OutboxMessageType(
    string name,
    Type messageType,
    Func<IServiceProvider, string, Guid, CancellationToken, Task> handle)
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public string Name { get; } = name;

    public Type MessageType { get; } = messageType;

    internal Task HandleAsync(IServiceProvider services, string payload, Guid messageId, CancellationToken cancellationToken)
        => handle(services, payload, messageId, cancellationToken);

    internal static OutboxMessageType For<TMessage>(string name)
        where TMessage : IOutboxMessage
        => new(name, typeof(TMessage), (services, payload, messageId, cancellationToken) =>
        {
            var message = JsonSerializer.Deserialize<TMessage>(payload, Json)
                ?? throw new InvalidOperationException($"Outbox message {messageId} has an empty payload.");
            return services.GetRequiredService<IOutboxMessageHandler<TMessage>>()
                .HandleAsync(message, messageId, cancellationToken);
        });
}
