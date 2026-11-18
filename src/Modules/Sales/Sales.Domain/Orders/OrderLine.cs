using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;
using Sales.Domain.Orders.Errors;

namespace Sales.Domain.Orders;

public sealed class OrderLine : BaseEntity<OrderLineId>
{
    public ProductId ProductId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal Amount { get; private set; }
    /// <summary>Per-unit discount (negative) or markup (positive) in UAH that a manager applied; already included in <see cref="Amount"/>.</summary>
    public decimal PriceAdjustment { get; private set; }

    private OrderLine() { }

    public static DomainResult<OrderLine> Create(ProductId productId, string productName, int quantity, decimal amount,
        decimal priceAdjustment = 0m)
    {
        if (productId.Value <= 0)
            return OrderErrors.ProductIdRequired();
        if (string.IsNullOrWhiteSpace(productName))
            return OrderErrors.ProductNameRequired();
        if (quantity <= 0)
            return OrderErrors.QuantityInvalid();
        if (amount < 0m)
            return OrderErrors.AmountInvalid();

        return new OrderLine
        {
            ProductId = productId,
            ProductName = productName.Trim(),
            Quantity = quantity,
            Amount = amount,
            PriceAdjustment = priceAdjustment,
        };
    }
}
