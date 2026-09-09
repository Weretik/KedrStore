using System.Reflection;
using Ardalis.Result;
using Sales.Application.Contracts.Catalog;
using Sales.Application.Contracts.Orders;
using Sales.Application.Contracts.Persistence;
using Sales.Application.Features.Orders.Create;
using Sales.Application.Features.Orders.Create.DTOs;
using Sales.Application.Features.Orders.Create.Validators;
using Sales.Domain.Orders.Entities;
using Sales.Domain.Orders.Enums;
using Sales.Domain.Orders.ValueObjects;

namespace UnitTests.Sales.Application;

public sealed class CreateOrderCommandHandlerTests
{
    private static readonly CreateOrderCommand Command = new(
        new CreateOrderRequest("counterparty", "Note", [new CreateOrderLineRequest("101", 2, 50m)]),
        Guid.Parse("14dc9a1b-97d0-49dd-97e8-b69bcd467094"));

    [Fact]
    public void Validator_RejectsInvalidPayload()
    {
        var result = new CreateOrderCommandValidator().Validate(new CreateOrderCommand(
            new CreateOrderRequest("", null, []), Guid.Empty));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Handle_PersistsPendingOrder_WhenRequestIsValid()
    {
        var store = new Store();
        var result = await CreateHandler(store).Handle(Command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value.OrderId);
        Assert.Equal("SO-20260902-0001", result.Value.OrderNumber);
        Assert.Equal(OneCOrderSyncStatus.Pending, result.Value.SyncStatus);
        Assert.True(store.Persisted);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenCounterpartyIsUnknown()
    {
        var store = new Store { CounterpartyExists = false };
        var result = await CreateHandler(store).Handle(Command, CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.False(store.Persisted);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_ForDifferentIdempotencyPayload()
    {
        var store = new Store { Existing = new CreateOrderIdempotencyRecord("create-admin-order", Command.IdempotencyKey.ToString("D"),
            "4A8A1537D7C1A11C81A09682956D124B331534DFB1A2DBEC1572F0A640B624CC", OrderId.Create(7), "SO-20260902-0007", OneCOrderSyncStatus.Pending, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(24)) };
        var result = await CreateHandler(store).Handle(Command, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.False(store.Persisted);
    }

    [Fact]
    public async Task Handle_ReturnsOriginalResult_ForMatchingIdempotencyKey()
    {
        var store = new Store();
        var handler = CreateHandler(store);
        var initial = await handler.Handle(Command, CancellationToken.None);
        store.Existing = store.LastIdempotency;

        var replay = await handler.Handle(Command, CancellationToken.None);

        Assert.True(initial.IsSuccess);
        Assert.True(replay.IsSuccess);
        Assert.Equal(initial.Value, replay.Value);
    }

    [Fact]
    public async Task Handle_PropagatesPersistenceFailure()
    {
        var store = new Store { ThrowOnPersist = true };

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler(store).Handle(Command, CancellationToken.None).AsTask());
    }

    private static CreateOrderCommandHandler CreateHandler(Store store) => new(store, new ProductReader(), new NumberGenerator());

    private sealed class NumberGenerator : IOrderNumberGenerator
    {
        public Task<string> GenerateAsync(DateTimeOffset createdAtUtc, CancellationToken cancellationToken) => Task.FromResult("SO-20260902-0001");
    }

    private sealed class ProductReader : IOrderProductReader
    {
        public Task<IReadOnlyDictionary<string, OrderProductSnapshot>> GetByIdsAsync(IReadOnlyCollection<string> productIds, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyDictionary<string, OrderProductSnapshot>>(new Dictionary<string, OrderProductSnapshot> { ["101"] = new("101", "Door") });
    }

    private sealed class Store : IOrderCreationStore
    {
        public bool CounterpartyExists { get; init; } = true;
        public bool ThrowOnPersist { get; init; }
        public bool Persisted { get; private set; }
        public CreateOrderIdempotencyRecord? Existing { get; set; }
        public CreateOrderIdempotencyRecord? LastIdempotency { get; private set; }
        public Task<bool> CounterpartyExistsAsync(string counterpartyId, CancellationToken cancellationToken) => Task.FromResult(CounterpartyExists);
        public Task<CreateOrderIdempotencyRecord?> FindIdempotencyRecordAsync(string operation, string idempotencyKey, CancellationToken cancellationToken) => Task.FromResult(Existing);
        public Task PersistAsync(Order order, Func<OrderId, OneCOrderSync> synchronizationFactory, Func<OrderId, CreateOrderIdempotencyRecord> idempotencyRecordFactory, CancellationToken cancellationToken)
        {
            if (ThrowOnPersist)
                throw new InvalidOperationException("Simulated transaction failure.");

            typeof(Order).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public)!.SetValue(order, OrderId.Create(42));
            LastIdempotency = idempotencyRecordFactory(order.Id);
            Persisted = true;
            return Task.CompletedTask;
        }
    }
}
