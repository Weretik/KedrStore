using Sales.Domain.Orders.Errors;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders.Entities;

public sealed class OrderLine : BaseEntity<OrderLineId>
{
    public string ProductId { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal Amount { get; private set; }
    /// <summary>Per-unit discount (negative) or markup (positive) in UAH that a manager applied; already included in <see cref="Amount"/>.</summary>
    public decimal PriceAdjustment { get; private set; }

    private OrderLine() { }

    public static DomainResult<OrderLine> Create(string productId, string productName, int quantity, decimal amount,
        decimal priceAdjustment = 0m)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return OrderErrors.ProductIdRequired();
        if (string.IsNullOrWhiteSpace(productName))
            return OrderErrors.ProductNameRequired();
        if (quantity <= 0)
            return OrderErrors.QuantityInvalid();
        if (amount < 0m)
            return OrderErrors.AmountInvalid();

        return new OrderLine
        {
            ProductId = productId.Trim(),
            ProductName = productName.Trim(),
            Quantity = quantity,
            Amount = amount,
            PriceAdjustment = priceAdjustment,
        };
    }
}
