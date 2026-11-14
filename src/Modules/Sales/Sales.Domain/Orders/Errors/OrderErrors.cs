using BuildingBlocks.Domain.Abstractions.Errors;

namespace Sales.Domain.Orders.Errors;

public static class OrderErrors
{
    public static DomainError CounterpartyIdRequired() => new("sales.order.counterparty_id_required", "Виберіть клієнта.", Field: "CounterpartyId");
    public static DomainError LinesRequired() => new("sales.order.lines_required", "Додайте хоча б один товар.", Field: "Lines");
    public static DomainError ProductIdRequired() => new("sales.order.product_id_required", "Потрібен код товару.", Field: "ProductId");
    public static DomainError ProductNameRequired() => new("sales.order.product_name_required", "Потрібна назва товару.", Field: "ProductName");
    public static DomainError QuantityInvalid() => new("sales.order.quantity_invalid", "Кількість має бути більшою за 0.", Field: "Quantity");
    public static DomainError AmountInvalid() => new("sales.order.amount_invalid", "Сума рядка не може бути від'ємною.", Field: "Amount");
    public static DomainError CommentTooLong() => new("sales.order.comment_too_long", $"Коментар — не більше {Order.CommentMaxLength} символів.", Field: "Comment");
    public static DomainError OrderNumberRequired() => new("sales.order.order_number_required", "Потрібен номер замовлення.", Field: "OrderNumber");
    public static DomainError OrderNumberTooLong() => new("sales.order.order_number_too_long", $"Номер замовлення — не більше {Order.OrderNumberMaxLength} символів.", Field: "OrderNumber");
    public static DomainError IdempotencyKeyRequired() => new("sales.order.idempotency_key_required", "Потрібен ключ повтору запиту.", Field: "IdempotencyKey");
    public static DomainError RequestHashRequired() => new("sales.order.request_hash_required", "Потрібен відбиток запиту.");
}
