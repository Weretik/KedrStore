using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Domain.QuickOrders;

namespace UnitTests.Sales.QuickOrders;

/// <summary>An in-memory <see cref="IQuickOrderStore"/> that records saves and the tokens they were given.</summary>
internal sealed class FakeQuickOrderStore : IQuickOrderStore
{
    private bool _raced;

    public List<QuickOrder> Saved { get; } = [];
    public List<QuickOrderPlaced> Messages { get; } = [];
    public List<CancellationToken> SaveTokens { get; } = [];

    /// <summary>An order with the same id that another request stores between our check and our save.</summary>
    public QuickOrder? RaceWinner { get; init; }

    /// <summary>The next resend save finds the row changed (another resend won).</summary>
    public bool ResendLosesTheRace { get; set; }

    public Task<QuickOrder?> FindAsync(QuickOrderId id, CancellationToken cancellationToken)
    {
        var found = Saved.FirstOrDefault(order => order.Id == id);
        if (found is null && _raced) found = RaceWinner;
        return Task.FromResult(found);
    }

    public Task<bool> TryAddAsync(QuickOrder order, QuickOrderPlaced message, CancellationToken cancellationToken)
    {
        SaveTokens.Add(cancellationToken);
        if (RaceWinner is not null)
        {
            _raced = true;
            return Task.FromResult(false);
        }

        Saved.Add(order);
        Messages.Add(message);
        return Task.FromResult(true);
    }

    public Task<bool> TrySaveResendAsync(QuickOrder order, QuickOrderPlaced message, CancellationToken cancellationToken)
    {
        if (ResendLosesTheRace)
            return Task.FromResult(false);
        Messages.Add(message);
        return Task.FromResult(true);
    }
}
