using Sales.Domain.Orders.Errors;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders.Entities;

public sealed class OrderLine : BaseEntity<OrderLineId>
{
    public string ProductId { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal Amount { get; private set; }

    private OrderLine() { }

    private OrderLine(string productId, string productName, int quantity, decimal amount)
    {
        ProductId = Required(productId, OrderErrors.ProductIdRequired());
        ProductName = Required(productName, OrderErrors.ProductNameRequired());

        if (quantity <= 0)
            throw new DomainException(OrderErrors.QuantityInvalid());

        if (amount < 0m)
            throw new DomainException(OrderErrors.AmountInvalid());

        Quantity = quantity;
        Amount = amount;
    }

    public static OrderLine Create(string productId, string productName, int quantity, decimal amount)
        => new(productId, productName, quantity, amount);

    private static string Required(string value, IDomainError error)
        => string.IsNullOrWhiteSpace(value) ? throw new DomainException(error) : value.Trim();
}
