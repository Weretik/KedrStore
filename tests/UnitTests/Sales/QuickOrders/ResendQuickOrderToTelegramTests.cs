using Ardalis.Result;
using Microsoft.Extensions.Logging.Abstractions;
using Sales.Application.Features.QuickOrders.Commands.ResendQuickOrderToTelegram;
using Sales.Domain.QuickOrders;

namespace UnitTests.Sales.QuickOrders;

/// <summary>Spec sales/009, R-008 and SC-007: resend only a failed order; a repeat sends nothing twice.</summary>
public sealed class ResendQuickOrderToTelegramTests
{
    private static readonly Guid Key = Guid.Parse("5a1c4e3b-8d7f-4a2e-b6c9-0e1f2a3b4c5d");
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task FailedOrder_IsQueuedAgain_Once()
    {
        var (store, order) = Stored();
        order.GiveUpTelegram("Bad Request: chat not found", Now);
        var handler = Handler(store);

        var first = await handler.Handle(new ResendQuickOrderToTelegramCommand(Key, "user-1"), CancellationToken.None);
        var repeat = await handler.Handle(new ResendQuickOrderToTelegramCommand(Key, "user-1"), CancellationToken.None);

        Assert.Equal(ResultStatus.Ok, first.Status);
        Assert.Equal(QuickOrderTelegramStatus.Pending, first.Value.TelegramStatus);
        Assert.Equal(Key, Assert.Single(store.Messages).OrderId);
        Assert.Equal(ResultStatus.Conflict, repeat.Status);
    }

    [Fact]
    public async Task SentOrder_AnswersConflict()
    {
        var (store, order) = Stored();
        order.MarkSentToTelegram(Now);

        var result = await Handler(store).Handle(new ResendQuickOrderToTelegramCommand(Key, "user-1"), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Empty(store.Messages);
    }

    [Fact]
    public async Task UnknownOrder_IsNotFound()
    {
        var result = await Handler(new FakeQuickOrderStore())
            .Handle(new ResendQuickOrderToTelegramCommand(Key, "user-1"), CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ParallelResend_ThatLosesTheRowVersion_AnswersConflict()
    {
        var (store, order) = Stored();
        order.GiveUpTelegram("error", Now);
        store.ResendLosesTheRace = true;

        var result = await Handler(store).Handle(new ResendQuickOrderToTelegramCommand(Key, "user-1"), CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Empty(store.Messages);
    }

    private static (FakeQuickOrderStore Store, QuickOrder Order) Stored()
    {
        var order = PlaceQuickOrderRepeatTests.Order(Key, quantity: 1);
        var store = new FakeQuickOrderStore();
        store.Saved.Add(order);
        return (store, order);
    }

    private static ResendQuickOrderToTelegramHandler Handler(FakeQuickOrderStore store) =>
        new(store, NullLogger<ResendQuickOrderToTelegramHandler>.Instance);
}
