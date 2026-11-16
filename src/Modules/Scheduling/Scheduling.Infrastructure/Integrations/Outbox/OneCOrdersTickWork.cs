using BuildingBlocks.Application.Outbox;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Features.Catalog;

namespace Scheduling.Infrastructure.Integrations.Outbox;

/// <summary>Orders to 1C: the outbox lane of its own, so the admin can pause or reschedule it apart from the general outbox.</summary>
internal sealed class OneCOrdersTickWork(IOutboxDispatcher dispatcher) : ITickWork
{
    public string TaskKey => ScheduledTaskKeys.OneCOrders;

    public async Task<string> RunAsync(CancellationToken cancellationToken)
    {
        var run = await dispatcher.DispatchLaneAsync(OutboxLanes.OneCOrders, cancellationToken);
        if (run.Failed > 0)
            throw new InvalidOperationException($"Не пішло замовлень: {run.Failed} (надіслано {run.Sent}, повториться {run.Retried}).");
        if (run.Sent == 0 && run.Retried == 0) return "Нічого відправляти.";
        return run.Retried == 0
            ? $"Надіслано замовлень: {run.Sent}."
            : $"Надіслано замовлень: {run.Sent}; повториться пізніше: {run.Retried}.";
    }
}
