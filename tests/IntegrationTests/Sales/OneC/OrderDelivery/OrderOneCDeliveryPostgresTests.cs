using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox.Dispatch;
using IntegrationTests.TestSupport.Database;
using IntegrationTests.TestSupport.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Abstractions.Integrations.OneC;
using Sales.Application.Abstractions.Orders.Sync;
using Sales.Application.Abstractions.Persistence;
using Sales.Application.Abstractions.Workspace.Orders;
using Sales.Application.Features.Orders.OneCDelivery.Messages;
using Sales.Domain.Customers;
using Sales.Domain.Orders;
using Sales.Infrastructure.Persistence;

namespace IntegrationTests.Sales.OneC.OrderDelivery;

/// <summary>Platform spec 006, stage 2: an order goes to 1C through the shared outbox (SC-008…SC-014).</summary>
[Collection(SalesPostgresCollection.Name)]
public sealed class OrderOneCDeliveryPostgresTests(SalesPostgresFixture postgres)
{
    [SkippableFact]
    public async Task NewOrder_IsStoredWithItsOutboxMessage_AndListedAsPending()
    {
        await using var sales = await SalesOutboxHarness.CreateAsync(postgres);

        var orderId = await CreateOrderAsync(sales);

        var message = Assert.Single(await sales.MessagesAsync(OrderPlaced.TypeName));
        Assert.Contains($"\"orderId\":{orderId}", message.Payload.Replace(" ", string.Empty));
        var row = await ListedAsync(sales, orderId);
        Assert.Equal("Pending", row.SyncStatus);
        Assert.Equal(0, row.SyncAttempts);
        Assert.Empty(sales.OneC.Sent);
    }

    [SkippableFact]
    public async Task Dispatch_AcceptedByOneC_StoresTheDocument()
    {
        await using var sales = await SalesOutboxHarness.CreateAsync(postgres);
        var orderId = await CreateOrderAsync(sales);
        sales.OneC.Answers.Enqueue(OneCOrderDeliveryResult.Accepted("DOC-1", null));

        await DispatchAllAsync(sales);

        var detail = await DetailAsync(sales, orderId);
        Assert.Equal("Accepted", detail.SyncStatus);
        Assert.Equal("DOC-1", detail.OneCDocumentNumber);
        Assert.Null(detail.SyncError);
        Assert.Equal(1, detail.SyncAttempts);
        Assert.NotNull((await sales.MessagesAsync(OrderPlaced.TypeName)).Single().ProcessedAtUtc);
    }

    [SkippableFact]
    public async Task Dispatch_RefusedByOneC_KeepsTheReason_AndIsNotRetried()
    {
        await using var sales = await SalesOutboxHarness.CreateAsync(postgres);
        var orderId = await CreateOrderAsync(sales);
        sales.OneC.Answers.Enqueue(OneCOrderDeliveryResult.BusinessError("Клієнта не знайдено"));

        await DispatchAllAsync(sales);
        sales.Clock.Advance(TimeSpan.FromHours(1));
        await DispatchAllAsync(sales);

        var row = await ListedAsync(sales, orderId);
        Assert.Equal("BusinessError", row.SyncStatus);
        Assert.Equal("Клієнта не знайдено", row.SyncError);
        Assert.Single(sales.OneC.Sent);
    }

    [SkippableFact]
    public async Task Dispatch_OneCUnreachable_ShowsTheAttemptTheErrorAndTheNextAttempt()
    {
        await using var sales = await SalesOutboxHarness.CreateAsync(postgres);
        var orderId = await CreateOrderAsync(sales);

        await DispatchAllAsync(sales);

        var row = await ListedAsync(sales, orderId);
        Assert.Equal("RetryScheduled", row.SyncStatus);
        Assert.Equal("1C is down", row.SyncError);
        Assert.Equal(1, row.SyncAttempts);
        Assert.Equal(SalesOutboxHarness.Noon, row.SyncLastAttemptAtUtc);
        Assert.Equal(SalesOutboxHarness.Noon.AddMinutes(1), row.SyncNextAttemptAtUtc);
    }

    [SkippableFact]
    public async Task TenFailures_GiveUp_AndTheManagersGetTheFileOnce()
    {
        await using var sales = await SalesOutboxHarness.CreateAsync(postgres);
        var orderId = await CreateOrderAsync(sales);

        await FailEveryAttemptAsync(sales);

        var row = await ListedAsync(sales, orderId);
        Assert.Equal("DeadLetter", row.SyncStatus);
        Assert.Equal(OutboxRetrySchedule.MaximumAttempts, row.SyncAttempts);
        Assert.Null(row.SyncNextAttemptAtUtc);
        var signal = Assert.Single(sales.Telegram.Sent);
        Assert.Equal(orderId, signal.OrderId);
        Assert.Equal(OutboxRetrySchedule.MaximumAttempts, signal.AttemptCount);
        Assert.Single(await sales.MessagesAsync(OrderOneCDeliveryFailed.TypeName));
    }

    [SkippableFact]
    public async Task Retry_AfterGivingUp_SendsAgain_Once_AndWritesTheAudit()
    {
        await using var sales = await SalesOutboxHarness.CreateAsync(postgres);
        var orderId = await CreateOrderAsync(sales);
        await FailEveryAttemptAsync(sales);

        var first = await RetryAsync(sales, orderId);
        var second = await RetryAsync(sales, orderId);

        Assert.Equal(OrderSyncRetryStoreOutcome.Scheduled, first.Outcome);
        Assert.Equal(OneCOrderSyncStatus.DeadLetter, first.PreviousStatus);
        Assert.Equal(OrderSyncRetryStoreOutcome.InvalidStatus, second.Outcome);
        Assert.Equal("Pending", (await ListedAsync(sales, orderId)).SyncStatus);
        Assert.Equal(1, await sales.InScopeAsync(services => services.GetRequiredService<SalesDbContext>().Database
            .SqlQueryRaw<int>("""SELECT COUNT(*)::integer AS "Value" FROM "OrderSyncRetryAudits" """).SingleAsync()));

        sales.OneC.Answers.Enqueue(OneCOrderDeliveryResult.Accepted("DOC-9", null));
        await DispatchAllAsync(sales);
        Assert.Equal("Accepted", (await ListedAsync(sales, orderId)).SyncStatus);
    }

    [SkippableFact]
    public async Task Retry_AfterARefusal_ClearsTheReason_AndRefusesAWaitingOrder()
    {
        await using var sales = await SalesOutboxHarness.CreateAsync(postgres);
        var refused = await CreateOrderAsync(sales);
        sales.OneC.Answers.Enqueue(OneCOrderDeliveryResult.BusinessError("Немає ціни"));
        await DispatchAllAsync(sales);
        var waiting = await CreateOrderAsync(sales, "cp-2");

        Assert.Equal(OrderSyncRetryStoreOutcome.Scheduled, (await RetryAsync(sales, refused)).Outcome);
        Assert.Equal(OrderSyncRetryStoreOutcome.InvalidStatus, (await RetryAsync(sales, waiting)).Outcome);

        var row = await ListedAsync(sales, refused);
        Assert.Equal("Pending", row.SyncStatus);
        Assert.Null(row.SyncError);
    }

    /// <summary>The order lane is sent by its own task, the rest (give-up signals) by the general one.</summary>
    private static async Task DispatchAllAsync(SalesOutboxHarness sales)
    {
        await sales.Dispatcher.DispatchLaneAsync(OutboxLanes.OneCOrders, CancellationToken.None);
        await sales.Dispatcher.DispatchDueAsync(CancellationToken.None);
    }

    private static async Task FailEveryAttemptAsync(SalesOutboxHarness sales)
    {
        for (var attempt = 0; attempt < OutboxRetrySchedule.MaximumAttempts; attempt++)
        {
            await DispatchAllAsync(sales);
            // Stay inside the sending window: an hour passes, then back to noon of the next day when needed.
            sales.Clock.Advance(TimeSpan.FromHours(1));
            if (attempt % 6 == 5)
                sales.Clock.Advance(TimeSpan.FromHours(18));
        }

        // The Telegram signal stored by the give-up goes out with the next dispatch.
        await DispatchAllAsync(sales);
    }

    private static Task<OrderSyncRetryStoreResult> RetryAsync(SalesOutboxHarness sales, long orderId)
        => sales.InScopeAsync(services => services.GetRequiredService<IOrderSyncRetryStore>()
            .ScheduleAsync(orderId, "Довідники виправлено", "manager-1", sales.Clock.GetUtcNow(), CancellationToken.None));

    private static async Task<long> CreateOrderAsync(SalesOutboxHarness sales, string counterpartyId = "cp-1")
        => await sales.InScopeAsync(async services =>
        {
            var db = services.GetRequiredService<SalesDbContext>();
            if (!await db.Counterparties.AnyAsync(x => x.Id == counterpartyId))
            {
                db.Counterparties.Add(Counterparty.Create(counterpartyId, Guid.NewGuid(), "Клієнт", $"{counterpartyId}@example.com",
                    null, 1, SalesOutboxHarness.Noon).Value);
                await db.SaveChangesAsync();
            }

            var order = Order.Create("P" + Guid.NewGuid().ToString("N")[..20], counterpartyId, null,
                [OrderLine.Create("101", "Ручка", 2, 50m).Value], SalesOutboxHarness.Noon).Value;
            var key = Guid.NewGuid().ToString("D");
            Assert.True(await services.GetRequiredService<IOrderCreationStore>().PersistAsync(order,
                id => new CreateOrderIdempotencyRecord("test", key, "hash", id, order.OrderNumber,
                    OneCOrderSyncStatus.Pending, SalesOutboxHarness.Noon, SalesOutboxHarness.Noon.AddHours(24)),
                CancellationToken.None));
            return order.Id.Value;
        });

    private static Task<WorkspaceOrderListItem> ListedAsync(SalesOutboxHarness sales, long orderId)
        => sales.InScopeAsync(async services =>
        {
            var page = await services.GetRequiredService<IWorkspaceOrderReader>()
                .GetOrdersAsync(null, null, null, null, "-createdAt", 1, 50, CancellationToken.None);
            return page.Value.Single(item => item.OrderId == orderId);
        });

    private static Task<WorkspaceOrderDetail> DetailAsync(SalesOutboxHarness sales, long orderId)
        => sales.InScopeAsync(async services =>
            (await services.GetRequiredService<IWorkspaceOrderReader>().GetOrderAsync(orderId, CancellationToken.None))!);
}
