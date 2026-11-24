
namespace Sales.Domain.QuickOrders;

/// <summary>One product of a site order as the buyer saw it in the cart; changed only through <see cref="QuickOrder"/>.</summary>
public sealed class QuickOrderItem : BaseEntity<QuickOrderItemId>
{
    public const int ProductIdMaxLength = 64;
    public const int TitleMaxLength = 500;

    /// <summary>The product id the site sent, kept as text: it is a copy of the cart, not a catalog reference.</summary>
    public string ProductId { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal Amount => decimal.Round(UnitPrice * Quantity, 2, MidpointRounding.AwayFromZero);

    private QuickOrderItem() { }

    public static DomainResult<QuickOrderItem> Create(string productId, string title, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return QuickOrderErrors.ProductIdRequired();
        if (string.IsNullOrWhiteSpace(title))
            return QuickOrderErrors.TitleRequired();
        if (quantity <= 0)
            return QuickOrderErrors.QuantityInvalid();
        if (unitPrice < 0m)
            return QuickOrderErrors.UnitPriceInvalid();

        return new QuickOrderItem
        {
            ProductId = productId.Trim(),
            Title = title.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity,
        };
    }

    /// <summary>The same product, title, price and quantity.</summary>
    public bool SameAs(QuickOrderItem other) =>
        ProductId == other.ProductId && Title == other.Title && UnitPrice == other.UnitPrice && Quantity == other.Quantity;
}
