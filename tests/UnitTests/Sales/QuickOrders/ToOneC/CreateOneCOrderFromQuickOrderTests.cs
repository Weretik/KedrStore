using System.Reflection;
using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Persistence;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Application.Features.QuickOrders.ToOneC;
using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;
using Sales.Domain.QuickOrders;

namespace UnitTests.Sales.QuickOrders.ToOneC;

/// <summary>
/// Spec sales/009, R-010: a site order becomes an ordinary 1C order on the retail counterparty, with the buyer's lines
/// and prices and a comment naming the buyer; a repeated message makes no second order.
/// </summary>
public sealed class CreateOneCOrderFromQuickOrderTests
{
    private const string Retail = "000001588";
    private static readonly DateTimeOffset Placed = new(2026, 11, 26, 10, 15, 0, TimeSpan.Zero);
    private static readonly Guid Key = Guid.Parse("52992220-6a4e-4d8b-9e21-3c5a7b9d1f04");

    [Fact]
    public async Task SiteOrder_BecomesAnOrderOnTheRetailCounterparty_AndIsLinked()
    {
        var (quickOrders, orders) = Stores(Order(("101", 125.50m, 2), ("000202", 10m, 3)));

        var result = await Handler(quickOrders, orders).HandleAsync(new QuickOrderToOneC(Key), Guid.NewGuid(), default);

        Assert.Equal(OutboxHandlingOutcome.Delivered, result.Outcome);
        var order = Assert.Single(orders.Added);
        Assert.Equal(Retail, order.CounterpartyId.Value);
        Assert.Equal("Сайт № 52992220, Іван, +380670000000, 26.11.2026 12:15", order.Comment);
        Assert.Equal([(101, 2, 251.00m, "Товар 101"), (202, 3, 30m, "Товар 000202")],
            order.Lines.Select(line => (line.ProductId.Value, line.Quantity, line.Amount, line.ProductName)));
        Assert.Equal(order.Id, quickOrders.Saved[0].OneCOrderId);
    }

    [Fact]
    public async Task RepeatedMessage_MakesNoSecondOrder()
    {
        var (quickOrders, orders) = Stores(Order(("101", 1m, 1)));
        var handler = Handler(quickOrders, orders);

        await handler.HandleAsync(new QuickOrderToOneC(Key), Guid.NewGuid(), default);
        var repeat = await handler.HandleAsync(new QuickOrderToOneC(Key), Guid.NewGuid(), default);

        Assert.Equal(OutboxHandlingOutcome.Delivered, repeat.Outcome);
        Assert.Single(orders.Added);
    }

    [Fact]
    public async Task MissingRetailCounterparty_FailsTheAttempt_AndAddsNothing()
    {
        var (quickOrders, orders) = Stores(Order(("101", 1m, 1)));
        orders.CounterpartyExists = false;

        var result = await Handler(quickOrders, orders).HandleAsync(new QuickOrderToOneC(Key), Guid.NewGuid(), default);

        Assert.Equal(OutboxHandlingOutcome.Failed, result.Outcome);
        Assert.Contains(Retail, result.Error);
        Assert.Empty(orders.Added);
        Assert.Null(quickOrders.Saved[0].OneCOrderId);
    }

    [Fact]
    public async Task ProductOutsideTheCatalog_FailsTheAttempt()
    {
        var (quickOrders, orders) = Stores(Order(("abc", 1m, 1)));

        var result = await Handler(quickOrders, orders).HandleAsync(new QuickOrderToOneC(Key), Guid.NewGuid(), default);

        Assert.Equal(OutboxHandlingOutcome.Failed, result.Outcome);
        Assert.Contains("abc", result.Error);
        Assert.Empty(orders.Added);
    }

    [Fact]
    public async Task GoneSiteOrder_IsDone()
    {
        var orders = new OrderStore();

        var result = await Handler(new FakeQuickOrderStore(), orders)
            .HandleAsync(new QuickOrderToOneC(Key), Guid.NewGuid(), default);

        Assert.Equal(OutboxHandlingOutcome.Delivered, result.Outcome);
        Assert.Empty(orders.Added);
    }

    private static (FakeQuickOrderStore, OrderStore) Stores(QuickOrder order)
    {
        var quickOrders = new FakeQuickOrderStore();
        quickOrders.Saved.Add(order);
        return (quickOrders, new OrderStore());
    }

    private static QuickOrder Order(params (string ProductId, decimal Price, int Quantity)[] lines) =>
        QuickOrder.Create(QuickOrderId.Create(Key), "Іван", "+380670000000",
            lines.Select(line => QuickOrderItem.Create(line.ProductId, $"Товар {line.ProductId}", line.Price, line.Quantity).Value),
            Placed).Value;

    private static CreateOneCOrderFromQuickOrderHandler Handler(FakeQuickOrderStore quickOrders, OrderStore orders) =>
        new(quickOrders, orders, new Numbers(), Options.Create(new SiteOrderOneCOptions()), TimeProvider.System,
            NullLogger<CreateOneCOrderFromQuickOrderHandler>.Instance);

    private sealed class OrderStore : IOrderCreationStore
    {
        public bool CounterpartyExists { get; set; } = true;
        public List<Order> Added { get; } = [];

        public Task<bool> CounterpartyExistsAsync(string counterpartyId, CancellationToken cancellationToken) =>
            Task.FromResult(CounterpartyExists && counterpartyId == Retail);

        public Task<CreateOrderIdempotencyRecord?> FindIdempotencyRecordAsync(
            string operation, string idempotencyKey, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<bool> PersistAsync(Order order, Func<OrderId, CreateOrderIdempotencyRecord> idempotencyRecordFactory,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task AddFromOutboxAsync(Order order, CancellationToken cancellationToken)
        {
            typeof(Order).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public)!
                .SetValue(order, OrderId.Create(Added.Count + 1));
            Added.Add(order);
            return Task.CompletedTask;
        }
    }

    private sealed class Numbers : IOrderNumberGenerator
    {
        public Task<string> GenerateAsync(DateTimeOffset createdAtUtc, CancellationToken cancellationToken) =>
            Task.FromResult("P-test");
    }
}
