using BuildingBlocks.Application.Outbox;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Features.Catalog;

namespace Scheduling.Infrastructure.Integrations.Outbox;

/// <summary>The outbox of every module, run by each tick (platform/007 R-04 a, platform/006).</summary>
internal sealed class OutboxTickWork(IOutboxDispatcher dispatcher) : ITickWork
{
    public string TaskKey => ScheduledTaskKeys.Outbox;

    public async Task<string> RunAsync(CancellationToken cancellationToken)
    {
        var run = await dispatcher.DispatchDueAsync(cancellationToken);
        if (run.Failed > 0)
            throw new InvalidOperationException($"Не пішло повідомлень: {run.Failed} (надіслано {run.Sent}, повториться {run.Retried}).");
        if (run.Sent == 0 && run.Retried == 0) return "Нічого досилати.";
        return run.Retried == 0
            ? $"Надіслано повідомлень: {run.Sent}."
            : $"Надіслано повідомлень: {run.Sent}; повториться пізніше: {run.Retried}.";
    }
}
