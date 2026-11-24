using Ardalis.Result;
using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging.Abstractions;
using Sales.Application.Features.QuickOrders.Commands.PlaceQuickOrder;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Domain.QuickOrders;

namespace UnitTests.Sales.QuickOrders;

/// <summary>
/// Spec sales/009, R-001…R-003 and the idempotency rules: the site sends one Idempotency-Key per order attempt. The order
/// is stored with its Telegram message; a repeat gets the first order id back and sends nothing again; a buyer who stops
/// waiting cannot cancel a valid order.
/// </summary>
public sealed class PlaceQuickOrderRepeatTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid Key = Guid.Parse("7d3f0c52-6a4e-4d8b-9e21-3c5a7b9d1f04");
    private static readonly QuickOrderLine Line = new("101", "Ручка Кедр", 125.50m, 2);

    [Fact]
    public async Task FirstOrder_WithKey_IsStoredUnderTheKey_WithItsMessage_AndSent()
    {
        var store = new FakeQuickOrderStore();
        var outbox = new Outbox();

        var result = await Handler(store, outbox).Handle(Command(Key), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Key, result.Value);
        var order = Assert.Single(store.Saved);
        Assert.Equal(QuickOrderId.Create(Key), order.Id);
        Assert.Equal(251.00m, order.Total);
        Assert.Equal(QuickOrderTelegramStatus.Pending, order.TelegramStatus);
        var message = Assert.Single(store.Messages);
        Assert.Equal(Key, message.OrderId);
        Assert.Equal(Line, Assert.Single(message.Lines));
        Assert.Equal(Key, Assert.Single(Assert.Single(outbox.Dispatched)));
    }

    [Fact]
    public async Task ValidOrder_IsSavedAndSent_EvenWhenTheRequestIsCancelled()
    {
        var store = new FakeQuickOrderStore();
        var outbox = new Outbox();
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        var result = await Handler(store, outbox).Handle(Command(Key), cancelled.Token);

        Assert.True(result.IsSuccess);
        Assert.Single(store.Saved);
        Assert.False(Assert.Single(store.SaveTokens).CanBeCanceled);
        Assert.False(Assert.Single(outbox.Tokens).CanBeCanceled);
    }

    [Fact]
    public async Task RepeatedOrder_WithTheSameKey_ReturnsTheFirstId_AndStoresAndSendsNothing()
    {
        var store = new FakeQuickOrderStore();
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
    public async Task RepeatedOrder_WithExtraSpaces_IsTheSameOrder()
    {
        var store = new FakeQuickOrderStore();
        var handler = Handler(store, new Outbox());
        await handler.Handle(Command(Key), CancellationToken.None);

        var repeat = await handler.Handle(Command(Key) with { FirstName = " Іван " }, CancellationToken.None);

        Assert.True(repeat.IsSuccess);
        Assert.Single(store.Saved);
    }

    [Fact]
    public async Task SameKey_WithOtherData_IsAConflict()
    {
        var store = new FakeQuickOrderStore();
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
        var store = new FakeQuickOrderStore { RaceWinner = winner };
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
        var store = new FakeQuickOrderStore();
        var handler = Handler(store, new Outbox());

        var first = await handler.Handle(Command(null), CancellationToken.None);
        var second = await handler.Handle(Command(null), CancellationToken.None);

        Assert.NotEqual(first.Value, second.Value);
        Assert.Equal(2, store.Saved.Count);
    }

    private static PlaceQuickOrderHandler Handler(FakeQuickOrderStore store, Outbox outbox) =>
        new(store, outbox, new FixedClock(), NullLogger<PlaceQuickOrderHandler>.Instance);

    private static PlaceQuickOrderCommand Command(Guid? key, int quantity = 2) =>
        new("Іван", "+380670000000", [Line with { Quantity = quantity }], key);

    internal static QuickOrder Order(Guid id, int quantity) =>
        QuickOrder.Create(
            QuickOrderId.Create(id), "Іван", "+380670000000",
            [QuickOrderItem.Create(Line.ProductId, Line.Title, Line.UnitPrice, quantity).Value], Now).Value;

    private sealed class Outbox : IOutboxDispatcher
    {
        public List<IReadOnlyCollection<Guid>> Dispatched { get; } = [];
        public List<CancellationToken> Tokens { get; } = [];

        public Task DispatchAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken cancellationToken)
        {
            Dispatched.Add(messageIds);
            Tokens.Add(cancellationToken);
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
