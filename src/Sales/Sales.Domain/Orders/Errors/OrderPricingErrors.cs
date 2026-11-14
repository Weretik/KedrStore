using Sales.Domain.Orders.Pricing;

namespace Sales.Domain.Orders.Errors;

/// <summary>
/// Why a manager's order line cannot be priced. The codes are the order workspace's public error codes, and the admin
/// shows the messages as they are.
/// </summary>
public static class OrderPricingErrors
{
    public static DomainError UnavailableProduct(string productName, decimal stock, int quantity) =>
        new("unavailable_product",
            $"Недостатньо товару «{productName}» на складі: доступно {stock}, потрібно {quantity}.");

    public static DomainError UnavailablePrice(string productName) =>
        new("unavailable_price", $"Для товару «{productName}» немає ціни в гривнях для цього клієнта.");

    public static DomainError PriceBelowMinimum(string productName, decimal basePrice, decimal unitPrice) =>
        new("price_below_minimum", basePrice <= OrderLinePricing.MinDiscountedUnitPrice
            ? $"Ціна «{productName}» {basePrice:0.00} ₴ — знижку застосувати не можна."
            : $"Ціна «{productName}» після знижки {unitPrice:0.00} ₴, а має бути щонайменше " +
              $"{OrderLinePricing.MinDiscountedUnitPrice:0.00} ₴ за шт. Найбільша знижка — " +
              $"{basePrice - OrderLinePricing.MinDiscountedUnitPrice:0.00} ₴.");

    public static DomainError AmountOverflow() =>
        new("amount_overflow", "Сума замовлення завелика. Зменште кількість.");
}
