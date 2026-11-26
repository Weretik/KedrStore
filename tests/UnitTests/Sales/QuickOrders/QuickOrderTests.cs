using Sales.Domain.Orders.ValueObjects;
using Sales.Domain.QuickOrders;

namespace UnitTests.Sales.QuickOrders;

/// <summary>Spec sales/009, SC-010 and R-005: the site order aggregate and its Telegram delivery state.</summary>
public sealed class QuickOrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 7, 45, 0, TimeSpan.Zero);
    private static readonly QuickOrderId Id = QuickOrderId.Create(Guid.Parse("0b9f7a7e-2b1c-4f7e-9c0e-2f2d7c1a5e10"));

    [Fact]
    public void Create_TrimsTheBuyer_AndSumsTheLines()
    {
        var order = Valid(Item("101", "Ручка", 125.50m, 2), Item("102", "Замок", 0.335m, 3));

        Assert.Equal("Ірина", order.FirstName);
        Assert.Equal("+380954315238", order.Phone);
        Assert.Equal(251.00m + 1.01m, order.Total);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(QuickOrderTelegramStatus.Pending, order.TelegramStatus);
    }

    [Theory]
    [InlineData("", "+380954315238", "sales.quick_order.first_name_required")]
    [InlineData("Ірина", " ", "sales.quick_order.phone_required")]
    public void Create_WithoutTheBuyer_Fails(string firstName, string phone, string code)
    {
        var result = QuickOrder.Create(Id, firstName, phone, [Item("101", "Ручка", 1m, 1)], Now);

        Assert.Equal(code, result.Error?.Code);
    }

    [Fact]
    public void Create_WithoutLines_Fails()
    {
        var result = QuickOrder.Create(Id, "Ірина", "+380954315238", [], Now);

        Assert.Equal("sales.quick_order.items_required", result.Error?.Code);
    }

    [Theory]
    [InlineData("", "Ручка", 1, 1, "sales.quick_order.product_id_required")]
    [InlineData("101", " ", 1, 1, "sales.quick_order.title_required")]
    [InlineData("101", "Ручка", 1, 0, "sales.quick_order.quantity_invalid")]
    [InlineData("101", "Ручка", -1, 1, "sales.quick_order.unit_price_invalid")]
    public void Item_WithInvalidValues_Fails(string productId, string title, decimal price, int quantity, string code)
    {
        Assert.Equal(code, QuickOrderItem.Create(productId, title, price, quantity).Error?.Code);
    }

    [Fact]
    public void FailedAttempt_KeepsTheError_ThenDeliveryClearsIt()
    {
        var order = Valid(Item("101", "Ручка", 1m, 1));

        order.RecordTelegramFailure("Forbidden: bot was kicked from the group chat", Now);
        Assert.Equal(QuickOrderTelegramStatus.Pending, order.TelegramStatus);
        Assert.Equal("Forbidden: bot was kicked from the group chat", order.TelegramLastError);

        order.MarkSentToTelegram(Now.AddMinutes(1));
        Assert.Equal(QuickOrderTelegramStatus.Sent, order.TelegramStatus);
        Assert.Equal(Now.AddMinutes(1), order.TelegramSentAtUtc);
        Assert.Null(order.TelegramLastError);
    }

    [Fact]
    public void LongError_IsShortened()
    {
        var order = Valid(Item("101", "Ручка", 1m, 1));

        order.RecordTelegramFailure(new string('x', 5_000), Now);

        Assert.Equal(QuickOrder.TelegramErrorMaxLength, order.TelegramLastError!.Length);
    }

    [Fact]
    public void Resend_IsAllowedOnlyAfterGiveUp()
    {
        var order = Valid(Item("101", "Ручка", 1m, 1));
        Assert.Equal("delivery_in_progress", order.RequestTelegramResend().Error?.Code);

        order.GiveUpTelegram("Bad Request: chat not found", Now);
        Assert.Equal(QuickOrderTelegramStatus.Failed, order.TelegramStatus);

        Assert.True(order.RequestTelegramResend().IsSuccess);
        Assert.Equal(QuickOrderTelegramStatus.Pending, order.TelegramStatus);
        Assert.Null(order.TelegramLastError);
        Assert.Equal("delivery_in_progress", order.RequestTelegramResend().Error?.Code);
    }

    [Fact]
    public void SentOrder_CannotBeResent_AndLaterFailuresDoNotTouchIt()
    {
        var order = Valid(Item("101", "Ручка", 1m, 1));
        order.MarkSentToTelegram(Now);

        order.RecordTelegramFailure("late error", Now.AddMinutes(1));
        order.GiveUpTelegram("late error", Now.AddMinutes(2));

        Assert.Equal(QuickOrderTelegramStatus.Sent, order.TelegramStatus);
        Assert.Null(order.TelegramLastError);
        Assert.Equal("already_sent", order.RequestTelegramResend().Error?.Code);
    }

    [Fact]
    public void SameAs_ComparesTheBuyerAndTheLinesInOrder()
    {
        var order = Valid(Item("101", "Ручка", 1m, 1), Item("102", "Замок", 2m, 1));

        Assert.True(order.SameAs(Valid(Item("101", "Ручка", 1.00m, 1), Item("102", "Замок", 2m, 1))));
        Assert.False(order.SameAs(Valid(Item("102", "Замок", 2m, 1), Item("101", "Ручка", 1m, 1))));
        Assert.False(order.SameAs(Valid(Item("101", "Ручка", 1m, 2), Item("102", "Замок", 2m, 1))));
    }

    [Fact]
    public void NewOrder_HasA1CMessage_AndIsLinkedToOne1COrderOnly()
    {
        var order = Valid(Item("101", "Ручка", 1m, 1));
        var first = OrderId.Create(7);

        Assert.NotNull(order.OneCOrderMessageId);
        Assert.Null(order.OneCOrderId);
        Assert.True(order.LinkOneCOrder(first).IsSuccess);
        Assert.True(order.LinkOneCOrder(first).IsSuccess);
        Assert.Equal("one_c_order_already_linked", order.LinkOneCOrder(OrderId.Create(8)).Error?.Code);
        Assert.Equal(first, order.OneCOrderId);
    }

    private static QuickOrder Valid(params QuickOrderItem[] items) =>
        QuickOrder.Create(Id, " Ірина ", " +380954315238 ", items, Now).Value;

    private static QuickOrderItem Item(string productId, string title, decimal price, int quantity) =>
        QuickOrderItem.Create(productId, title, price, quantity).Value;
}
