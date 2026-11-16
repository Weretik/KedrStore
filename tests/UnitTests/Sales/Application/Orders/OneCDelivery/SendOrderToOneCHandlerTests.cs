using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sales.Application.Abstractions.DeliveryFailure;
using Sales.Application.Abstractions.Integrations.OneC;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.Orders.OneCDelivery.Messages;
using Sales.Application.Features.Orders.OneCDelivery.Send;
using Sales.Domain.Orders;

namespace UnitTests.Sales.Application.Orders.OneCDelivery;

/// <summary>Platform spec 006, R-011: SC-009…SC-012.</summary>
public sealed class SendOrderToOneCHandlerTests
{
    // 12:00 in Kyiv.
    private static readonly DateTimeOffset Noon = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Accepted_StoresTheDocumentOnTheOrder()
    {
        var (handler, order, oneC) = Create(OneCOrderDeliveryResult.Accepted("DOC-7", null));

        var result = await handler.HandleAsync(new OrderPlaced(7), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(OutboxHandlingResult.Delivered, result);
        Assert.Equal("DOC-7", order.OneCDocumentNumber);
        Assert.Equal("SO-7", oneC.Sent.Single().OrderNumber);
    }

    [Fact]
    public async Task BusinessError_IsStoredOnTheOrder_AndNotRetried()
    {
        var (handler, order, _) = Create(OneCOrderDeliveryResult.BusinessError("Немає\nціни"));

        var result = await handler.HandleAsync(new OrderPlaced(7), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(OutboxHandlingResult.Delivered, result);
        Assert.Equal("Немаєціни", order.OneCRejection);
    }

    [Fact]
    public async Task TransportError_IsAFailedAttempt()
    {
        var (handler, order, _) = Create(OneCOrderDeliveryResult.TransportError("timeout"));

        var result = await handler.HandleAsync(new OrderPlaced(7), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(OutboxHandlingOutcome.Failed, result.Outcome);
        Assert.Equal("timeout", result.Error);
        Assert.Null(order.OneCDocumentNumber);
        Assert.Null(order.OneCRejection);
    }

    [Fact]
    public async Task AtNight_NothingIsSent_UntilTheWindowOpens()
    {
        var (handler, _, oneC) = Create(OneCOrderDeliveryResult.Accepted("DOC-7", null), Noon.AddHours(12));

        var result = await handler.HandleAsync(new OrderPlaced(7), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(OutboxHandlingResult.RetryAt(new DateTimeOffset(2026, 11, 17, 5, 0, 0, TimeSpan.Zero)), result);
        Assert.Empty(oneC.Sent);
    }

    [Fact]
    public async Task AnOrderAlreadyAnswered_OrGone_IsNotSentAgain()
    {
        var (handler, order, oneC) = Create(OneCOrderDeliveryResult.Accepted("DOC-7", null));
        order.AcceptByOneC("DOC-7", Noon);

        Assert.Equal(OutboxHandlingResult.Delivered,
            await handler.HandleAsync(new OrderPlaced(7), Guid.NewGuid(), CancellationToken.None));
        Assert.Equal(OutboxHandlingResult.Delivered,
            await handler.HandleAsync(new OrderPlaced(8), Guid.NewGuid(), CancellationToken.None));
        Assert.Empty(oneC.Sent);
    }

    private static (SendOrderToOneCHandler Handler, Order Order, FakeOneC OneC) Create(
        OneCOrderDeliveryResult answer,
        DateTimeOffset? now = null)
    {
        var order = Order.Create("SO-7", "customer-1", null, [OrderLine.Create("p-1", "Ручка", 2, 20m).Value], Noon).Value;
        var oneC = new FakeOneC(answer);
        var handler = new SendOrderToOneCHandler(new FakeStore(order), oneC,
            new OneCSendingWindow(Options.Create(new OneCSendingWindowOptions())),
            new FixedClock(now ?? Noon), NullLogger<SendOrderToOneCHandler>.Instance);
        return (handler, order, oneC);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeStore(Order order) : IOrderOneCDeliveryStore
    {
        public Task<Order?> FindForDeliveryAsync(long orderId, CancellationToken cancellationToken)
            => Task.FromResult(orderId == 7 ? order : null);

        public void AddDeliveryFailedSignal(OrderOneCDeliveryFailed signal) => throw new NotSupportedException();

        public Task<DeadLetterNotification?> BuildFailureNotificationAsync(
            OrderOneCDeliveryFailed signal,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class FakeOneC(OneCOrderDeliveryResult answer) : ISalesOneCWriteClient
    {
        public List<OneCOrderDeliveryRequest> Sent { get; } = [];

        public Task<OneCOrderDeliveryResult> SendOrderAsync(OneCOrderDeliveryRequest request, CancellationToken cancellationToken)
        {
            Sent.Add(request);
            return Task.FromResult(answer);
        }
    }
}
