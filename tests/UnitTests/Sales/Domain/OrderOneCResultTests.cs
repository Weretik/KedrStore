using UnitTests.BuildingBlocks.Results;
using Sales.Domain.Orders;

namespace UnitTests.Sales.Domain;

/// <summary>Platform spec 006, R-011 and R-013: the 1C answer is stored on the order.</summary>
public sealed class OrderOneCResultTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_GivesTheOrderAStableOneCMessageId()
    {
        var order = NewOrder();

        Assert.NotEqual(Guid.Empty, order.OneCMessageId);
        Assert.NotEqual(NewOrder().OneCMessageId, order.OneCMessageId);
    }

    [Fact]
    public void AcceptByOneC_StoresTheDocument_AndClearsAnOldRejection()
    {
        var order = NewOrder();
        order.RejectByOneC("Немає ціни", Now);

        DomainAssert.Succeeds(order.AcceptByOneC(" DOC-1 ", Now.AddHours(1)));

        Assert.Equal("DOC-1", order.OneCDocumentNumber);
        Assert.Equal(Now.AddHours(1), order.OneCAcceptedAtUtc);
        Assert.Null(order.OneCRejection);
        Assert.Null(order.OneCRejectedAtUtc);
    }

    [Fact]
    public void AcceptByOneC_KeepsTheFirstDocumentNumber()
    {
        var order = NewOrder();
        order.AcceptByOneC("DOC-1", Now);

        DomainAssert.Succeeds(order.AcceptByOneC("DOC-1", Now.AddMinutes(5)));
        var changed = DomainAssert.Fails(order.AcceptByOneC("DOC-2", Now.AddMinutes(5)));

        Assert.Equal("sales.order.one_c_document_number_changed", changed.Code);
        Assert.Equal(Now, order.OneCAcceptedAtUtc);
    }

    [Fact]
    public void AcceptByOneC_WithoutADocumentNumber_Fails()
    {
        var error = DomainAssert.Fails(NewOrder().AcceptByOneC(" ", Now));

        Assert.Equal("sales.order.one_c_document_number_required", error.Code);
    }

    [Fact]
    public void RejectByOneC_KeepsTheReason_CutToTheLimit()
    {
        var order = NewOrder();

        DomainAssert.Succeeds(order.RejectByOneC(new string('x', Order.OneCRejectionMaxLength + 10), Now));

        Assert.Equal(Order.OneCRejectionMaxLength, order.OneCRejection!.Length);
        Assert.Equal(Now, order.OneCRejectedAtUtc);
    }

    [Fact]
    public void ReopenOneCDelivery_ClearsTheRejection_ButNotAnAcceptedOrder()
    {
        var rejected = NewOrder();
        rejected.RejectByOneC(null, Now);
        DomainAssert.Succeeds(rejected.ReopenOneCDelivery(Now));
        Assert.Null(rejected.OneCRejection);

        var accepted = NewOrder();
        accepted.AcceptByOneC("DOC-1", Now);
        Assert.Equal("sales.order.already_accepted_by_one_c", DomainAssert.Fails(accepted.ReopenOneCDelivery(Now)).Code);
        Assert.Equal("sales.order.already_accepted_by_one_c", DomainAssert.Fails(accepted.RejectByOneC("late", Now)).Code);
    }

    private static Order NewOrder()
        => Order.Create("SO-1", "customer-1", null, [OrderLine.Create("p-1", "Ручка", 1, 10m).Value], Now).Value;
}
