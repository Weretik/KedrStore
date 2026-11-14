using BuildingBlocks.Application.Outbox;
namespace BuildingBlocks.Infrastructure.Outbox.Dispatch;

/// <summary>The outbox table of one module.</summary>
internal interface IModuleOutbox
{
    /// <summary>Sends due messages, only the given ones when <paramref name="messageIds"/> is set.</summary>
    Task<OutboxDispatchReport> ProcessAsync(IReadOnlyCollection<Guid>? messageIds, CancellationToken cancellationToken);

    Task<int> RemoveProcessedAsync(CancellationToken cancellationToken);
}
