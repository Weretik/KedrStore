using Sales.Domain.Orders.Pricing;

namespace UnitTests.Sales.Domain;

public sealed class OrderLinePricingTests
{
    [Fact]
    public void Price_AppliesTheAdjustmentToEachUnit()
    {
        var line = OrderLinePricing.Price("Ручка", stock: 10, basePrice: 200m, quantity: 2, priceAdjustment: -50m).Value;

        Assert.Equal(new PricedLine(200m, 150m, 300m), line);
    }

    [Theory]
    [InlineData(1, 2, "unavailable_product")]
    [InlineData(5, 1, null)]
    public void Price_NeedsEnoughStock(decimal stock, int quantity, string? code)
        => Assert.Equal(code, OrderLinePricing.Price("Ручка", stock, 10m, quantity, 0m).Error?.Code);

    [Fact]
    public void Price_NeedsTheCustomersPrice()
        => Assert.Equal("unavailable_price", OrderLinePricing.Price("Ручка", 5, null, 1, 0m).Error?.Code);

    [Theory]
    [InlineData(200, -199, null)]
    [InlineData(200, -199.01, "price_below_minimum")]
    [InlineData(0.5, -0.01, "price_below_minimum")]
    [InlineData(0.5, 0, null)]
    public void Price_KeepsADiscountedUnitAtOneHryvnia(double basePrice, double adjustment, string? code)
        => Assert.Equal(code,
            OrderLinePricing.Price("Ручка", 5, (decimal)basePrice, 1, (decimal)adjustment).Error?.Code);

    [Fact]
    public void Price_RejectsAnAmountThatDoesNotFit()
        => Assert.Equal("amount_overflow",
            OrderLinePricing.Price("Ручка", int.MaxValue, decimal.MaxValue / 2, 3, 0m).Error?.Code);

    [Fact]
    public void Total_RejectsASumThatDoesNotFit()
        => Assert.Equal("amount_overflow",
            OrderLinePricing.Total([decimal.MaxValue, 1m]).Error?.Code);
}
