using BuildingBlocks.Domain.Exceptions;
using Sales.Domain.Orders.Entities;

namespace UnitTests;

public sealed class SalesOrderTests
{
    [Fact]
    public void Create_CreatesImmutableOrderWithHistoricalLineSnapshot()
    {
        var line = OrderLine.Create("000123", "Door handle", 10, 1_250m);
        var order = Order.Create("SO-20260817-0001", "customer-1", "Urgent", [line], DateTimeOffset.UtcNow);

        Assert.Equal("SO-20260817-0001", order.OrderNumber);
        Assert.Equal("customer-1", order.CounterpartyId);
        var savedLine = Assert.Single(order.Lines);
        Assert.Equal("Door handle", savedLine.ProductName);
        Assert.Equal(10, savedLine.Quantity);
        Assert.Equal(1_250m, savedLine.Amount);
    }

    [Fact]
    public void Create_RejectsEmptyLines()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Order.Create("SO-20260817-0001", "customer-1", null, [], DateTimeOffset.UtcNow));

        Assert.Equal("sales.order.lines_required", exception.Error.Code);
    }

    [Theory]
    [InlineData("", "Name", 1, 10)]
    [InlineData("product", "", 1, 10)]
    [InlineData("product", "Name", 0, 10)]
    [InlineData("product", "Name", 1, -0.01)]
    public void CreateLine_RejectsInvalidValues(string productId, string name, int quantity, decimal amount)
    {
        Assert.Throws<DomainException>(() => OrderLine.Create(productId, name, quantity, amount));
    }

    [Fact]
    public void Create_RejectsOrderNumberLongerThan32Characters()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Order.Create(new string('A', 33), "customer-1", null, [OrderLine.Create("p", "Name", 1, 1m)], DateTimeOffset.UtcNow));

        Assert.Equal("sales.order.order_number_too_long", exception.Error.Code);
    }
}
