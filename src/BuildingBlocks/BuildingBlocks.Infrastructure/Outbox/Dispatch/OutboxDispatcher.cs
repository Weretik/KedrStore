using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging;
namespace BuildingBlocks.Infrastructure.Outbox.Dispatch;

/// <summary>Sends the outbox messages of every module that registered an outbox table.</summary>
internal sealed class OutboxDispatcher(IEnumerable<IModuleOutbox> modules, ILogger<OutboxDispatcher> logger)
    : IOutboxDispatcher
{
    public async Task DispatchAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken cancellationToken)
    {
        if (messageIds.Count == 0)
            return;

        foreach (var module in modules)
        {
            try
            {
                await module.ProcessAsync(messageIds, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                // R-003: the request already committed its data; the dispatcher sends the messages later.
                logger.LogWarning(exception, "Outbox attempt right after the commit failed; the dispatcher will retry.");
            }
        }
    }

    public async Task<OutboxDispatchReport> DispatchDueAsync(CancellationToken cancellationToken)
    {
        var report = OutboxDispatchReport.Empty;
        foreach (var module in modules)
        {
            report = report.Add(await module.ProcessAsync(messageIds: null, cancellationToken));
            await module.RemoveProcessedAsync(cancellationToken);
        }

        return report;
    }
}
