using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders;

public sealed class OrderDraftLine
{
    public OrderDraftLineId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public int Quantity { get; private set; }
    /// <summary>Per-unit discount (negative) or markup (positive) in UAH; editable until the order is placed.</summary>
    public decimal PriceAdjustment { get; private set; }

    private OrderDraftLine() { }
    private OrderDraftLine(ProductId productId, int quantity, decimal priceAdjustment)
    {
        if (productId.Value <= 0)
            throw new ArgumentException("Product ID is invalid.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        ProductId = productId;
        Quantity = quantity;
        PriceAdjustment = priceAdjustment;
    }

    public static OrderDraftLine Create(ProductId productId, int quantity, decimal priceAdjustment = 0m)
        => new(productId, quantity, priceAdjustment);
}