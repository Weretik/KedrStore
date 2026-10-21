using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders.Entities;

public sealed class OrderDraftLine
{
    public OrderDraftLineId Id { get; private set; }
    public string ProductId { get; private set; } = null!;
    public int Quantity { get; private set; }

    private OrderDraftLine() { }
    private OrderDraftLine(string productId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId) || productId.Length > 64)
            throw new ArgumentException("Product ID is invalid.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        ProductId = productId.Trim();
        Quantity = quantity;
    }

    public static OrderDraftLine Create(string productId, int quantity) => new(productId, quantity);
}