using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Enums;

namespace IntegrationTests.TestSupport.Database;

internal sealed class ConcurrentClaimBarrierInterceptor(AsyncBarrier barrier) : SaveChangesInterceptor
{
    private bool _claimIntercepted;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var claim = eventData.Context?.ChangeTracker
            .Entries<OneCOrderSync>()
            .SingleOrDefault(entry =>
                entry.State == EntityState.Modified &&
                entry.Entity.Status == OneCOrderSyncStatus.Sent);

        if (_claimIntercepted || claim is null)
            return result;

        _claimIntercepted = true;
        await barrier.SignalAndWaitAsync(cancellationToken);
        return result;
    }
}

internal sealed class AsyncBarrier(int participantCount)
{
    private readonly TaskCompletionSource _allArrived = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _remaining = participantCount;

    public async Task SignalAndWaitAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Decrement(ref _remaining) == 0)
            _allArrived.TrySetResult();

        await _allArrived.Task.WaitAsync(cancellationToken);
    }
}
