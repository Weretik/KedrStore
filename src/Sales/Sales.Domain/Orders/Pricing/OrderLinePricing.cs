using Sales.Domain.Orders.Errors;

namespace Sales.Domain.Orders.Pricing;

/// <summary>
/// The price of one order line a manager builds: the product must be in stock and have the customer's UAH price, a
/// discount may not take a unit below 1 ₴ (a product already cheaper keeps its price), and the amount must fit.
/// </summary>
public static class OrderLinePricing
{
    /// <summary>Largest per-unit discount or markup, in UAH, a manager may enter.</summary>
    public const decimal MaxPriceAdjustment = 1_000_000m;

    /// <summary>Lowest unit price, in UAH, a discount may bring a product to.</summary>
    public const decimal MinDiscountedUnitPrice = 1m;

    public static DomainResult<PricedLine> Price(
        string productName, decimal stock, decimal? basePrice, int quantity, decimal priceAdjustment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);

        if (stock < quantity)
            return OrderPricingErrors.UnavailableProduct(productName, stock, quantity);
        if (basePrice is not { } price)
            return OrderPricingErrors.UnavailablePrice(productName);
        var unitPrice = price + priceAdjustment;
        if (priceAdjustment < 0m && unitPrice < Math.Min(MinDiscountedUnitPrice, price))
            return OrderPricingErrors.PriceBelowMinimum(productName, price, unitPrice);
        try
        {
            return new PricedLine(price, unitPrice, checked(unitPrice * quantity));
        }
        catch (OverflowException)
        {
            return OrderPricingErrors.AmountOverflow();
        }
    }

    /// <summary>The order total, or an error when it does not fit.</summary>
    public static DomainResult<decimal> Total(IEnumerable<decimal> lineAmounts)
    {
        ArgumentNullException.ThrowIfNull(lineAmounts);
        try
        {
            return lineAmounts.Aggregate(0m, (sum, amount) => checked(sum + amount));
        }
        catch (OverflowException)
        {
            return OrderPricingErrors.AmountOverflow();
        }
    }
}

/// <param name="BaseUnitPrice">The customer's price for one unit before the manager's adjustment.</param>
/// <param name="UnitPrice">The price for one unit after the adjustment.</param>
public sealed record PricedLine(decimal BaseUnitPrice, decimal UnitPrice, decimal LineAmount);
