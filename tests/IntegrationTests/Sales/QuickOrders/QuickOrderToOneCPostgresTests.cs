using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Domain.Identifiers;
using Identity.Contracts.Users;
using IntegrationTests.TestSupport.Database;
using IntegrationTests.TestSupport.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Abstractions.Integrations.OneC;
using Sales.Application.Abstractions.QuickOrders;
using Sales.Application.Features.Orders.OneCDelivery.Messages;
using Sales.Application.Features.QuickOrders.Messages;
using Sales.Domain.Customers;
using Sales.Domain.QuickOrders;
using Sales.Infrastructure.Persistence;

namespace IntegrationTests.Sales.QuickOrders;

/// <summary>
/// Spec sales/009, R-010: a stored site order becomes an ordinary order on the retail counterparty through the outbox,
/// and that order goes to 1C like any other.
/// </summary>
[Collection(SalesPostgresCollection.Name)]
public sealed class QuickOrderToOneCPostgresTests(SalesPostgresFixture postgres)
{
    private const string Retail = "000001588";

    [SkippableFact]
    public async Task SiteOrder_BecomesARetailOrder_ThatGoesTo1C()
    {
        await using var sales = await CreateAsync(withRetail: true);
        var quickOrder = await PlaceAsync(sales);

        // Both messages of the order go in one dispatch batch, as right after the site request.
        await sales.Dispatcher.DispatchAsync([quickOrder.Id.Value, quickOrder.OneCOrderMessageId!.Value], CancellationToken.None);

        var (order, linked) = await sales.InScopeAsync(async services =>
        {
            var db = services.GetRequiredService<SalesDbContext>();
            var made = await db.Orders.AsNoTracking().Include(x => x.Lines).SingleAsync();
            var link = await db.Set<QuickOrder>().AsNoTracking().Where(x => x.Id == quickOrder.Id)
                .Select(x => x.OneCOrderId).SingleAsync();
            return (made, link);
        });
        Assert.Equal(Retail, order.CounterpartyId.Value);
        Assert.StartsWith("SO-", order.OrderNumber);
        Assert.Equal("Сайт № " + quickOrder.Id.Value.ToString("N")[..8].ToUpperInvariant() + ", Іван, +380670000000, 16.11.2026 12:00",
            order.Comment);
        var line = Assert.Single(order.Lines);
        Assert.Equal((101, 2, 251.00m), (line.ProductId.Value, line.Quantity, line.Amount));
        Assert.Equal(order.Id, linked);
        Assert.NotNull(Assert.Single(await sales.MessagesAsync(QuickOrderToOneC.TypeName)).ProcessedAtUtc);

        sales.OneC.Otherwise = OneCOrderDeliveryResult.Accepted("DOC-1", null);
        await sales.Dispatcher.DispatchLaneAsync(OutboxLanes.OneCOrders, CancellationToken.None);

        var sent = Assert.Single(sales.OneC.Sent);
        Assert.Equal(Retail, sent.CounterpartyId);
        Assert.Equal(order.Comment, sent.Comment);
        Assert.NotNull(Assert.Single(await sales.MessagesAsync(OrderPlaced.TypeName)).ProcessedAtUtc);
    }

    [SkippableFact]
    public async Task WithoutTheRetailCounterparty_TheAttemptFails_AndNoOrderIsMade()
    {
        await using var sales = await CreateAsync(withRetail: false);
        var quickOrder = await PlaceAsync(sales);

        await sales.Dispatcher.DispatchAsync([quickOrder.OneCOrderMessageId!.Value], CancellationToken.None);

        var message = Assert.Single(await sales.MessagesAsync(QuickOrderToOneC.TypeName));
        Assert.Null(message.ProcessedAtUtc);
        Assert.Equal(1, message.Attempts);
        Assert.Contains(Retail, message.LastError);
        Assert.Equal(0, await sales.InScopeAsync(services => services.GetRequiredService<SalesDbContext>().Orders.CountAsync()));
    }

    private async Task<SalesOutboxHarness> CreateAsync(bool withRetail)
    {
        var sales = await SalesOutboxHarness.CreateAsync(postgres);
        await sales.InScopeAsync(async services =>
        {
            var db = services.GetRequiredService<SalesDbContext>();
            await db.Set<QuickOrder>().ExecuteDeleteAsync();
            if (withRetail)
            {
                db.Counterparties.Add(Counterparty.Create(Retail, UserId.FromStorage(Guid.NewGuid()), "РОЗНИЦА ХАЛТУРИНА",
                    "retail@example.com", null, PriceTypeId.FromStorage(1), SalesOutboxHarness.Noon).Value);
                await db.SaveChangesAsync();
            }

            return 0;
        });
        return sales;
    }

    private static Task<QuickOrder> PlaceAsync(SalesOutboxHarness sales) => sales.InScopeAsync(async services =>
    {
        var order = QuickOrder.Create(QuickOrderId.New(SalesOutboxHarness.Noon), "Іван", "+380670000000",
            [QuickOrderItem.Create("101", "Ручка Кедр", 125.50m, 2).Value], SalesOutboxHarness.Noon).Value;
        Assert.True(await services.GetRequiredService<IQuickOrderStore>().TryAddAsync(
            order, QuickOrderPlaced.From(order), new QuickOrderToOneC(order.Id.Value), CancellationToken.None));
        return order;
    });
}
