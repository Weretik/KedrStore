using Ardalis.Result;
using BuildingBlocks.Application.Outbox;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;
using Sales.Application.Features.QuickOrders.Messages;

namespace UnitTests.Sales.QuickOrders;

/// <summary>
/// Idempotency rules: the site sends one Idempotency-Key per order attempt. A repeat (a second press, a retry after
/// a lost answer) gets the first order id back and sends nothing to Telegram again.
/// </summary>
public sealed class PlaceQuickOrderRepeatTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid Key = Guid.Parse("7d3f0c52-6a4e-4d8b-9e21-3c5a7b9d1f04");
    private static readonly QuickOrderLine Line = new("101", "Ручка Кедр", 125.50m, 2);

    [Fact]
    public async Task FirstOrder_WithKey_IsStoredUnderTheKey_AndSent()
    {
        var store = new Store();
        var outbox = new Outbox();

        var result = await Handler(store, outbox).Handle(Command(Key), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Key, result.Value);
        Assert.Equal(Key, Assert.Single(store.Saved).OrderId);
        Assert.Equal(Key, Assert.Single(Assert.Single(outbox.Dispatched)));
    }

    [Fact]
    public async Task RepeatedOrder_WithTheSameKey_ReturnsTheFirstId_AndStoresAndSendsNothing()
    {
        var store = new Store();
        var outbox = new Outbox();
        var handler = Handler(store, outbox);
        await handler.Handle(Command(Key), CancellationToken.None);

        var repeat = await handler.Handle(Command(Key), CancellationToken.None);

        Assert.True(repeat.IsSuccess);
        Assert.Equal(Key, repeat.Value);
        Assert.Single(store.Saved);
        Assert.Single(outbox.Dispatched);
    }

    [Fact]
    public async Task SameKey_WithOtherData_IsAConflict()
    {
        var store = new Store();
        var handler = Handler(store, new Outbox());
        await handler.Handle(Command(Key), CancellationToken.None);

        var other = await handler.Handle(Command(Key, quantity: 5), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, other.Status);
        Assert.Contains(other.Errors, message => message.Contains("іншими даними"));
        Assert.Single(store.Saved);
    }

    [Fact]
    public async Task SecondCopy_ThatLosesTheRace_ReturnsTheWinnersId_AndSendsNothing()
    {
        var winner = Order(Key, quantity: 2);
        var store = new Store { RaceWinner = winner };
        var outbox = new Outbox();

        var result = await Handler(store, outbox).Handle(Command(Key), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Key, result.Value);
        Assert.Empty(store.Saved);
        Assert.Empty(outbox.Dispatched);
    }

    [Fact]
    public async Task OrdersWithoutKey_AreSeparateOrders()
    {
        var store = new Store();
        var handler = Handler(store, new Outbox());

        var first = await handler.Handle(Command(null), CancellationToken.None);
        var second = await handler.Handle(Command(null), CancellationToken.None);

        Assert.NotEqual(first.Value, second.Value);
        Assert.Equal(2, store.Saved.Count);
    }

    private static PlaceQuickOrderHandler Handler(Store store, Outbox outbox) =>
        new(store, outbox, new FixedClock());

    private static PlaceQuickOrderCommand Command(Guid? key, int quantity = 2) =>
        new("Іван", "+380670000000", [Line with { Quantity = quantity }], key);

    private static QuickOrderPlaced Order(Guid id, int quantity) =>
        new(id, "Іван", "+380670000000", [Line with { Quantity = quantity }], Now);

    private sealed class Store : IQuickOrderStore
    {
        public List<QuickOrderPlaced> Saved { get; } = [];

        /// <summary>An order with the same id that another request stores between our check and our save.</summary>
        public QuickOrderPlaced? RaceWinner { get; init; }

        private bool _raced;

        public Task<QuickOrderPlaced?> FindAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var found = Saved.FirstOrDefault(x => x.OrderId == orderId);
            if (found is null && _raced) found = RaceWinner;
            return Task.FromResult(found);
        }

        public Task<bool> TrySaveAsync(QuickOrderPlaced order, CancellationToken cancellationToken)
        {
            if (RaceWinner is not null)
            {
                _raced = true;
                return Task.FromResult(false);
            }

            Saved.Add(order);
            return Task.FromResult(true);
        }
    }

    private sealed class Outbox : IOutboxDispatcher
    {
        public List<IReadOnlyCollection<Guid>> Dispatched { get; } = [];

        public Task DispatchAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken cancellationToken)
        {
            Dispatched.Add(messageIds);
            return Task.CompletedTask;
        }

        public Task<OutboxDispatchReport> DispatchDueAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OutboxDispatchReport.Empty);

        public Task<OutboxDispatchReport> DispatchLaneAsync(string lane, CancellationToken cancellationToken) =>
            Task.FromResult(OutboxDispatchReport.Empty);
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
